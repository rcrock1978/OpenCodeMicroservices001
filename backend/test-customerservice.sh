#!/usr/bin/env bash
#
# CustomerService Endpoint Test Script
# Tests all API endpoints for the Customer Service directly
#
# Usage:
#   chmod +x test-customerservice.sh
#   ./test-customerservice.sh
#
# Prerequisites:
#   - CustomerService must be running (default port 5005)
#   - curl and jq must be installed
#

set -euo pipefail

# ── Configuration ──────────────────────────────────────────────────────────────
BASE_URL="http://localhost:5005"
DEMO_TENANT_ID="11111111-1111-1111-1111-111111111111"

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
echo "║          CUSTOMER SERVICE ENDPOINT TEST SUITE                        ║"
echo "║          Target: $BASE_URL                              ║"
echo "╚══════════════════════════════════════════════════════════════════════╝"
echo ""

# ── 0. Service Health ────────────────────────────────────────────────────────
log_step "Service Health"
http_get "$BASE_URL/" "Root endpoint"
http_get "$BASE_URL/health/live" "Health: live"
http_get "$BASE_URL/health/ready" "Health: ready"

# ── 1. Customers ─────────────────────────────────────────────────────────────
log_step "Customer Endpoints"

# List all customers
http_get "$BASE_URL/api/customers" "List all customers"
CUSTOMER_LIST=$(http_get_body "$BASE_URL/api/customers")
CUSTOMER_COUNT=$(echo "$CUSTOMER_LIST" | jq 'length' 2>/dev/null || echo "0")
log_info "Found $CUSTOMER_COUNT existing customers"

# Get customers by tenant
http_get "$BASE_URL/api/customers/tenant/$DEMO_TENANT_ID" "List customers by tenant"

# Get a specific customer (first from list)
EXISTING_CUSTOMER_ID=$(echo "$CUSTOMER_LIST" | jq -r '.[0].id // empty' 2>/dev/null || true)
if [[ -n "$EXISTING_CUSTOMER_ID" && "$EXISTING_CUSTOMER_ID" != "null" ]]; then
  http_get "$BASE_URL/api/customers/$EXISTING_CUSTOMER_ID" "Get customer by ID ($EXISTING_CUSTOMER_ID)"
else
  log_warn "No existing customer to test GET by ID"
fi

# Test 404 for non-existent customer
http_expect "GET" "$BASE_URL/api/customers/00000000-0000-0000-0000-000000000000" "404" "Get non-existent customer (expect 404)"

# Create a customer
log_info "Creating a test customer..."
CREATE_CUSTOMER_RESPONSE=$(http_post_body "$BASE_URL/api/customers" \
  "{\"tenantId\":\"$DEMO_TENANT_ID\",\"userId\":null,\"email\":\"test.customer.$(date +%s)@example.com\",\"firstName\":\"Test\",\"lastName\":\"Customer\",\"phoneNumber\":\"+1-555-1234\"}")

NEW_CUSTOMER_ID=$(echo "$CREATE_CUSTOMER_RESPONSE" | jq -r '.id // empty' 2>/dev/null || true)
if [[ -n "$NEW_CUSTOMER_ID" && "$NEW_CUSTOMER_ID" != "null" ]]; then
  log_pass "Create customer -> ID: $NEW_CUSTOMER_ID"

  # Verify customer exists
  http_get "$BASE_URL/api/customers/$NEW_CUSTOMER_ID" "Get created customer"

  # Verify customer appears in tenant list
  TENANT_CUSTOMERS=$(http_get_body "$BASE_URL/api/customers/tenant/$DEMO_TENANT_ID")
  FOUND=$(echo "$TENANT_CUSTOMERS" | jq "map(select(.id == \"$NEW_CUSTOMER_ID\")) | length" 2>/dev/null || echo "0")
  if [[ "$FOUND" -gt 0 ]]; then
    log_pass "Verify customer in tenant list"
  else
    log_fail "Verify customer in tenant list"
  fi
else
  log_fail "Create customer -> No ID returned"
fi

# ── 2. Addresses ─────────────────────────────────────────────────────────────
log_step "Address Endpoints"

if [[ -n "$NEW_CUSTOMER_ID" && "$NEW_CUSTOMER_ID" != "null" ]]; then
  # Get addresses for customer (should be empty initially)
  ADDRESSES=$(http_get_body "$BASE_URL/api/addresses/customer/$NEW_CUSTOMER_ID")
  ADDR_COUNT=$(echo "$ADDRESSES" | jq 'length' 2>/dev/null || echo "0")
  log_info "Customer $NEW_CUSTOMER_ID has $ADDR_COUNT addresses"

  # Create an address
  log_info "Creating a test address..."
  CREATE_ADDRESS_RESPONSE=$(curl -s -X POST -H "Content-Type: application/json" \
    -d "{\"customerId\":\"$NEW_CUSTOMER_ID\",\"type\":0,\"street\":\"123 Test Street\",\"city\":\"Test City\",\"state\":\"CA\",\"postalCode\":\"90210\",\"country\":\"US\",\"isDefault\":true}" \
    "$BASE_URL/api/addresses" || true)

  CREATE_ADDR_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST -H "Content-Type: application/json" \
    -d "{\"customerId\":\"$NEW_CUSTOMER_ID\",\"type\":0,\"street\":\"123 Test Street\",\"city\":\"Test City\",\"state\":\"CA\",\"postalCode\":\"90210\",\"country\":\"US\",\"isDefault\":true}" \
    "$BASE_URL/api/addresses" || true)

  NEW_ADDRESS_ID=$(echo "$CREATE_ADDRESS_RESPONSE" | jq -r '.id // empty' 2>/dev/null || true)
  if [[ -n "$NEW_ADDRESS_ID" && "$NEW_ADDRESS_ID" != "null" ]]; then
    log_pass "Create address -> ID: $NEW_ADDRESS_ID"

    # Verify address appears in customer addresses
    ADDR_LIST=$(http_get_body "$BASE_URL/api/addresses/customer/$NEW_CUSTOMER_ID")
    ADDR_FOUND=$(echo "$ADDR_LIST" | jq "map(select(.id == \"$NEW_ADDRESS_ID\")) | length" 2>/dev/null || echo "0")
    if [[ "$ADDR_FOUND" -gt 0 ]]; then
      log_pass "Verify address in customer address list"
    else
      log_fail "Verify address in customer address list"
    fi
  else
    log_fail "Create address -> No ID returned (HTTP $CREATE_ADDR_STATUS)"
    if [[ -n "$CREATE_ADDRESS_RESPONSE" ]]; then
      log_warn "Response: $CREATE_ADDRESS_RESPONSE"
    fi
  fi
else
  log_warn "Skipping address tests - no valid customer ID"
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
  echo -e "${GREEN}All CustomerService tests passed!${NC}"
  exit 0
fi
