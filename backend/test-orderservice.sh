#!/usr/bin/env bash
#
# OrderService Endpoint Test Script
# Tests all API endpoints for the Order Service directly
#
# Usage:
#   chmod +x test-orderservice.sh
#   ./test-orderservice.sh
#
# Prerequisites:
#   - OrderService must be running (default port 5004)
#   - curl and jq must be installed
#

set -euo pipefail

# ── Configuration ──────────────────────────────────────────────────────────────
BASE_URL="http://localhost:5004"
DEMO_TENANT_ID="11111111-1111-1111-1111-111111111111"
DEMO_CUSTOMER_ID="aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"
DEMO_PRODUCT_ID="00000000-0000-0000-0000-000000000001"

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
echo "║          ORDER SERVICE ENDPOINT TEST SUITE                             ║"
echo "║          Target: $BASE_URL                              ║"
echo "╚══════════════════════════════════════════════════════════════════════╝"
echo ""

# ── 0. Service Health ────────────────────────────────────────────────────────
log_step "Service Health"
http_get "$BASE_URL/" "Root endpoint"
http_get "$BASE_URL/health/live" "Health: live"
http_get "$BASE_URL/health/ready" "Health: ready"

# ── 1. Orders ────────────────────────────────────────────────────────────────
log_step "Order Endpoints"

# List all orders
http_get "$BASE_URL/api/orders" "List all orders"
ORDER_LIST=$(http_get_body "$BASE_URL/api/orders")
ORDER_COUNT=$(echo "$ORDER_LIST" | jq 'length' 2>/dev/null || echo "0")
log_info "Found $ORDER_COUNT orders"

# List orders by tenant
http_get "$BASE_URL/api/orders/tenant/$DEMO_TENANT_ID" "List orders by tenant"

# Get a specific order (first from list)
EXISTING_ORDER_ID=$(echo "$ORDER_LIST" | jq -r '.[0].id // empty' 2>/dev/null || true)
if [[ -n "$EXISTING_ORDER_ID" && "$EXISTING_ORDER_ID" != "null" ]]; then
  http_get "$BASE_URL/api/orders/$EXISTING_ORDER_ID" "Get order by ID ($EXISTING_ORDER_ID)"

  # Verify order has items
  ITEMS_COUNT=$(echo "$ORDER_LIST" | jq '.[0].items | length' 2>/dev/null || echo "0")
  log_info "First order has $ITEMS_COUNT items"
else
  log_warn "No existing orders found"
fi

# Test 404 for non-existent order
http_expect "GET" "$BASE_URL/api/orders/00000000-0000-0000-0000-000000000000" "404" "Get non-existent order (expect 404)"

# Create an order
log_info "Creating a test order..."
NEW_ITEM_ID=$(uuidgen 2>/dev/null || echo "00000000-0000-0000-0000-000000000002")
CREATE_ORDER_RESPONSE=$(http_post_body "$BASE_URL/api/orders" \
  "{\"tenantId\":\"$DEMO_TENANT_ID\",\"customerId\":\"$DEMO_CUSTOMER_ID\",\"items\":[{\"productId\":\"$DEMO_PRODUCT_ID\",\"productVariantId\":\"$NEW_ITEM_ID\",\"productName\":\"Test Product\",\"sku\":\"TEST-001\",\"unitPrice\":29.99,\"quantity\":2}],\"shippingCost\":5.00,\"taxAmount\":2.40,\"currency\":\"USD\",\"shippingAddress\":\"123 Test St, Test City, US\"}")

NEW_ORDER_ID=$(echo "$CREATE_ORDER_RESPONSE" | jq -r '.id // empty' 2>/dev/null || true)
if [[ -n "$NEW_ORDER_ID" && "$NEW_ORDER_ID" != "null" ]]; then
  log_pass "Create order -> ID: $NEW_ORDER_ID"

  # Verify order exists
  http_get "$BASE_URL/api/orders/$NEW_ORDER_ID" "Get created order"

  # Verify order appears in tenant list
  TENANT_ORDERS=$(http_get_body "$BASE_URL/api/orders/tenant/$DEMO_TENANT_ID")
  FOUND=$(echo "$TENANT_ORDERS" | jq "map(select(.id == \"$NEW_ORDER_ID\")) | length" 2>/dev/null || echo "0")
  if [[ "$FOUND" -gt 0 ]]; then
    log_pass "Verify order in tenant list"
  else
    log_fail "Verify order in tenant list"
  fi

  # Verify order total was calculated
  ORDER_TOTAL=$(echo "$CREATE_ORDER_RESPONSE" | jq -r '.total // empty' 2>/dev/null || true)
  if [[ -n "$ORDER_TOTAL" && "$ORDER_TOTAL" != "null" && "$ORDER_TOTAL" != "0" ]]; then
    log_pass "Verify order total calculated -> $ORDER_TOTAL"
  else
    log_fail "Verify order total calculated"
  fi

  # Cancel the order (if not already shipped/delivered)
  log_info "Cancelling order $NEW_ORDER_ID..."
  CANCEL_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE_URL/api/orders/$NEW_ORDER_ID/cancel" || true)
  if [[ "$CANCEL_STATUS" == "200" ]]; then
    log_pass "Cancel order -> HTTP 200"

    # Verify status is Cancelled
    CANCELLED_ORDER=$(http_get_body "$BASE_URL/api/orders/$NEW_ORDER_ID")
    CANCELLED_STATUS=$(echo "$CANCELLED_ORDER" | jq -r '.status // empty' 2>/dev/null || true)
    if [[ "$CANCELLED_STATUS" == "Cancelled" ]]; then
      log_pass "Verify order status -> Cancelled"
    else
      log_warn "Verify order status -> $CANCELLED_STATUS (expected 'Cancelled')"
    fi
  elif [[ "$CANCELLED_STATUS" == "400" ]]; then
    log_warn "Cancel order -> Order already shipped/delivered (HTTP 400)"
  else
    log_fail "Cancel order -> HTTP $CANCEL_STATUS"
  fi
else
  log_fail "Create order -> No ID returned"
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
  echo -e "${GREEN}All OrderService tests passed!${NC}"
  exit 0
fi
