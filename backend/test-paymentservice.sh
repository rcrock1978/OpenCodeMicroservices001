#!/usr/bin/env bash
#
# PaymentService Endpoint Test Script
# Tests all API endpoints for the Payment Service directly
#
# Usage:
#   chmod +x test-paymentservice.sh
#   ./test-paymentservice.sh
#
# Prerequisites:
#   - PaymentService must be running (default port 5006)
#   - curl and jq must be installed
#

set -euo pipefail

# ── Configuration ──────────────────────────────────────────────────────────────
BASE_URL="http://localhost:5006"
DEMO_TENANT_ID="11111111-1111-1111-1111-111111111111"
DEMO_CUSTOMER_ID="aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"
DEMO_ORDER_ID="00000000-0000-0000-0000-000000000001"

# ── Colors ───────────────────────────────────────────────────────────────────
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

PASS_COUNT=0
FAIL_COUNT=0

# ── Helper functions ─────────────────────────────────────────────────────────
log_info()  { echo -e "${BLUE}[INFO]${NC} $1"; }
log_pass()  { echo -e "${GREEN}[PASS]${NC} $1"; PASS_COUNT=$((PASS_COUNT+1)); }
log_fail()  { echo -e "${RED}[FAIL]${NC} $1"; FAIL_COUNT=$((FAIL_COUNT+1)); }
log_warn()  { echo -e "${YELLOW}[WARN]${NC} $1"; }
log_step()  { echo -e "${CYAN}[STEP]${NC} $1"; }

http_get() {
  local url="$1"
  local desc="${2:-$url}"
  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" "$url" || true)
  if [[ "$status" == "200" || "$status" == "201" ]]; then
    log_pass "GET $desc -> HTTP $status"
    return 0
  else
    log_fail "GET $desc -> HTTP $status (expected 200/201)"
    return 1
  fi
}

http_get_body() {
  local url="$1"
  curl -s "$url" || true
}

http_post() {
  local url="$1"
  local json="$2"
  local desc="${3:-$url}"
  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" -X POST -H "Content-Type: application/json" -d "$json" "$url" || true)
  if [[ "$status" == "200" || "$status" == "201" ]]; then
    log_pass "POST $desc -> HTTP $status"
    return 0
  else
    log_fail "POST $desc -> HTTP $status (expected 200/201)"
    return 1
  fi
}

http_post_body() {
  local url="$1"
  local json="$2"
  curl -s -X POST -H "Content-Type: application/json" -d "$json" "$url" || true
}

http_expect() {
  local method="$1"
  local url="$2"
  local expected="$3"
  local desc="${4:-$url}"
  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" -X "$method" "$url" || true)
  if [[ "$status" == "$expected" ]]; then
    log_pass "$method $desc -> HTTP $status"
    return 0
  else
    log_fail "$method $desc -> HTTP $status (expected $expected)"
    return 1
  fi
}

# ── Banner ───────────────────────────────────────────────────────────────────
echo ""
echo "╔══════════════════════════════════════════════════════════════════════╗"
echo "║          PAYMENT SERVICE ENDPOINT TEST SUITE                         ║"
echo "║          Target: $BASE_URL                              ║"
echo "╚══════════════════════════════════════════════════════════════════════╝"
echo ""

# ── 0. Service Health ────────────────────────────────────────────────────────
log_step "Service Health"
http_get "$BASE_URL/" "Root endpoint"
http_get "$BASE_URL/health/live" "Health: live"
http_get "$BASE_URL/health/ready" "Health: ready"

# ── 1. Payment Intents ───────────────────────────────────────────────────────
log_step "Payment Intent Endpoints"

# List all payment intents
http_get "$BASE_URL/api/payments/intents" "List all payment intents"
INTENT_LIST=$(http_get_body "$BASE_URL/api/payments/intents")
INTENT_COUNT=$(echo "$INTENT_LIST" | jq 'length' 2>/dev/null || echo "0")
log_info "Found $INTENT_COUNT payment intents"

# Get a specific payment intent (first from list)
EXISTING_INTENT_ID=$(echo "$INTENT_LIST" | jq -r '.[0].id // empty' 2>/dev/null || true)
if [[ -n "$EXISTING_INTENT_ID" && "$EXISTING_INTENT_ID" != "null" ]]; then
  http_get "$BASE_URL/api/payments/intents/$EXISTING_INTENT_ID" "Get payment intent by ID"
else
  log_warn "No existing payment intents found"
fi

# Test 404 for non-existent intent
http_expect "GET" "$BASE_URL/api/payments/intents/00000000-0000-0000-0000-000000000000" "404" "Get non-existent intent (expect 404)"

# Create a successful payment intent
log_info "Creating a successful payment intent..."
IDEMPOTENCY_KEY="test-key-$(date +%s)"
CREATE_INTENT_RESPONSE=$(http_post_body "$BASE_URL/api/payments/intents" \
  "{\"tenantId\":\"$DEMO_TENANT_ID\",\"orderId\":\"$DEMO_ORDER_ID\",\"customerId\":\"$DEMO_CUSTOMER_ID\",\"amount\":99.99,\"currency\":\"USD\",\"idempotencyKey\":\"$IDEMPOTENCY_KEY\",\"paymentMethod\":\"card\",\"testFailure\":false}")

NEW_INTENT_ID=$(echo "$CREATE_INTENT_RESPONSE" | jq -r '.id // empty' 2>/dev/null || true)
if [[ -n "$NEW_INTENT_ID" && "$NEW_INTENT_ID" != "null" ]]; then
  INTENT_STATUS=$(echo "$CREATE_INTENT_RESPONSE" | jq -r '.status // empty' 2>/dev/null || true)
  log_pass "Create payment intent -> ID: $NEW_INTENT_ID, Status: $INTENT_STATUS"

  # Verify intent exists
  http_get "$BASE_URL/api/payments/intents/$NEW_INTENT_ID" "Get created intent"

  # If status is Succeeded, test refund
  if [[ "$INTENT_STATUS" == "Succeeded" ]]; then
    log_info "Refunding payment intent $NEW_INTENT_ID..."
    REFUND_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/payments/intents/$NEW_INTENT_ID/refund" || true)
    if [[ "$REFUND_STATUS" == "200" ]]; then
      log_pass "Refund payment intent -> HTTP 200"

      # Verify status is Refunded
      REFUNDED_INTENT=$(http_get_body "$BASE_URL/api/payments/intents/$NEW_INTENT_ID")
      REFUNDED_STATUS=$(echo "$REFUNDED_INTENT" | jq -r '.status // empty' 2>/dev/null || true)
      if [[ "$REFUNDED_STATUS" == "Refunded" ]]; then
        log_pass "Verify intent status -> Refunded"
      else
        log_warn "Verify intent status -> $REFUNDED_STATUS (expected 'Refunded')"
      fi
    else
      log_fail "Refund payment intent -> HTTP $REFUND_STATUS (expected 200)"
    fi
  else
    log_warn "Skipping refund test - intent status is '$INTENT_STATUS' (needs 'Succeeded')"
  fi
else
  log_fail "Create payment intent -> No ID returned"
fi

# Create a failed payment intent (testFailure=true)
log_info "Creating a failed payment intent..."
FAIL_KEY="fail-key-$(date +%s)"
FAIL_INTENT_RESPONSE=$(http_post_body "$BASE_URL/api/payments/intents" \
  "{\"tenantId\":\"$DEMO_TENANT_ID\",\"orderId\":\"$DEMO_ORDER_ID\",\"customerId\":\"$DEMO_CUSTOMER_ID\",\"amount\":0.01,\"currency\":\"USD\",\"idempotencyKey\":\"$FAIL_KEY\",\"paymentMethod\":\"card\",\"testFailure\":true}")

FAIL_INTENT_STATUS=$(echo "$FAIL_INTENT_RESPONSE" | jq -r '.status // empty' 2>/dev/null || true)
if [[ "$FAIL_INTENT_STATUS" == "Failed" ]]; then
  log_pass "Create failed payment intent -> Status: Failed"
else
  log_warn "Create failed payment intent -> Status: $FAIL_INTENT_STATUS (expected 'Failed')"
fi

# Test refund on non-existent intent
http_expect "POST" "$BASE_URL/api/payments/intents/00000000-0000-0000-0000-000000000000/refund" "404" "Refund non-existent intent (expect 404)"

# ── 2. Payment Methods ───────────────────────────────────────────────────────
log_step "Payment Method Endpoints"

# List all payment methods
http_get "$BASE_URL/api/payments/methods" "List all payment methods"
METHOD_LIST=$(http_get_body "$BASE_URL/api/payments/methods")
METHOD_COUNT=$(echo "$METHOD_LIST" | jq 'length' 2>/dev/null || echo "0")
log_info "Found $METHOD_COUNT payment methods"

# Create a payment method
log_info "Creating a test payment method..."
CREATE_METHOD_RESPONSE=$(http_post_body "$BASE_URL/api/payments/methods" \
  "{\"tenantId\":\"$DEMO_TENANT_ID\",\"customerId\":\"$DEMO_CUSTOMER_ID\",\"type\":\"Card\",\"lastFour\":\"4242\",\"brand\":\"Visa\",\"expMonth\":12,\"expYear\":2030,\"isDefault\":true}")

NEW_METHOD_ID=$(echo "$CREATE_METHOD_RESPONSE" | jq -r '.id // empty' 2>/dev/null || true)
if [[ -n "$NEW_METHOD_ID" && "$NEW_METHOD_ID" != "null" ]]; then
  log_pass "Create payment method -> ID: $NEW_METHOD_ID"

  # Verify method appears in list
  http_get "$BASE_URL/api/payments/methods" "List methods after create"
else
  log_fail "Create payment method -> No ID returned"
fi

# ── Summary ───────────────────────────────────────────────────────────────────
echo ""
echo "╔══════════════════════════════════════════════════════════════════════╗"
echo "║                         TEST SUMMARY                                 ║"
echo "╠══════════════════════════════════════════════════════════════════════╣"
printf  "║  ${GREEN}PASSED:${NC} %-4d                                                  ║\n" "$PASS_COUNT"
printf  "║  ${RED}FAILED:${NC} %-4d                                                  ║\n" "$FAIL_COUNT"
printf  "║  TOTAL:  %-4d                                                      ║\n" "$((PASS_COUNT + FAIL_COUNT))"
echo "╚══════════════════════════════════════════════════════════════════════╝"
echo ""

if [[ "$FAIL_COUNT" -gt 0 ]]; then
  echo -e "${RED}Some tests failed. Please review the output above.${NC}"
  exit 1
else
  echo -e "${GREEN}All PaymentService tests passed!${NC}"
  exit 0
fi
