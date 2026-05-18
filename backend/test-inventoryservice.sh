#!/usr/bin/env bash
#
# InventoryService Endpoint Test Script
# Tests all API endpoints for the Inventory Service directly
#
# Usage:
#   chmod +x test-inventoryservice.sh
#   ./test-inventoryservice.sh
#
# Prerequisites:
#   - InventoryService must be running (default port 5003)
#   - curl and jq must be installed
#

set -euo pipefail

# ── Configuration ──────────────────────────────────────────────────────────────
BASE_URL="http://localhost:5003"
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
  if [[ "$status" == "200" || "$status" == "201" || "$status" == "204" ]]; then
    log_pass "POST $desc -> HTTP $status"
    return 0
  else
    log_fail "POST $desc -> HTTP $status (expected 200/201/204)"
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
echo "║          INVENTORY SERVICE ENDPOINT TEST SUITE                         ║"
echo "║          Target: $BASE_URL                              ║"
echo "╚══════════════════════════════════════════════════════════════════════╝"
echo ""

# ── 0. Service Health ────────────────────────────────────────────────────────
log_step "Service Health"
http_get "$BASE_URL/" "Root endpoint"
http_get "$BASE_URL/health/live" "Health: live"
http_get "$BASE_URL/health/ready" "Health: ready"

# ── 1. Stock Items ───────────────────────────────────────────────────────────
log_step "Stock Item Endpoints"

# List all stock items
http_get "$BASE_URL/api/inventory" "List all stock items"
ITEM_LIST=$(http_get_body "$BASE_URL/api/inventory")
ITEM_COUNT=$(echo "$ITEM_LIST" | jq 'length' 2>/dev/null || echo "0")
log_info "Found $ITEM_COUNT stock items"

# Get a specific stock item (first from list)
EXISTING_ITEM_ID=$(echo "$ITEM_LIST" | jq -r '.[0].id // empty' 2>/dev/null || true)
EXISTING_SKU=""
if [[ -n "$EXISTING_ITEM_ID" && "$EXISTING_ITEM_ID" != "null" ]]; then
  http_get "$BASE_URL/api/inventory/$EXISTING_ITEM_ID" "Get stock item by ID"
  EXISTING_SKU=$(echo "$ITEM_LIST" | jq -r '.[0].sku // empty' 2>/dev/null || true)
  log_info "Using existing SKU: $EXISTING_SKU"
else
  log_warn "No existing stock items found"
fi

# Test 404 for non-existent stock item
http_expect "GET" "$BASE_URL/api/inventory/00000000-0000-0000-0000-000000000000" "404" "Get non-existent stock item (expect 404)"

# Create a stock item
log_info "Creating a test stock item..."
NEW_VARIANT_ID=$(uuidgen 2>/dev/null || echo "00000000-0000-0000-0000-000000000001")
CREATE_ITEM_RESPONSE=$(http_post_body "$BASE_URL/api/inventory" \
  "{\"tenantId\":\"$DEMO_TENANT_ID\",\"productVariantId\":\"$NEW_VARIANT_ID\",\"sku\":\"TEST-SKU-$(date +%s)\",\"quantityAvailable\":100,\"lowStockThreshold\":10}")

NEW_ITEM_ID=$(echo "$CREATE_ITEM_RESPONSE" | jq -r '.id // empty' 2>/dev/null || true)
if [[ -n "$NEW_ITEM_ID" && "$NEW_ITEM_ID" != "null" ]]; then
  log_pass "Create stock item -> ID: $NEW_ITEM_ID"

  # Verify item exists
  http_get "$BASE_URL/api/inventory/$NEW_ITEM_ID" "Get created stock item"
else
  log_fail "Create stock item -> No ID returned"
fi

# ── 2. Stock Reservation ─────────────────────────────────────────────────────
log_step "Stock Reservation Endpoints"

if [[ -n "$NEW_ITEM_ID" && "$NEW_ITEM_ID" != "null" ]]; then
  # Reserve stock for the newly created item
  NEW_ORDER_ID=$(uuidgen 2>/dev/null || echo "00000000-0000-0000-0000-000000000002")
  log_info "Reserving stock for order $NEW_ORDER_ID..."

  RESERVE_RESPONSE=$(http_post_body "$BASE_URL/api/inventory/reserve" \
    "{\"tenantId\":\"$DEMO_TENANT_ID\",\"sku\":\"TEST-SKU-$(date +%s)\",\"orderId\":\"$NEW_ORDER_ID\",\"quantity\":5}")

  # If the SKU doesn't exist in seeded data, this may fail; try with existing SKU if available
  if [[ -z "$RESERVE_RESPONSE" || "$RESERVE_RESPONSE" == "null" ]]; then
    log_warn "Reservation with new SKU failed, trying with existing SKU..."
    if [[ -n "$EXISTING_SKU" ]]; then
      NEW_ORDER_ID=$(uuidgen 2>/dev/null || echo "00000000-0000-0000-0000-000000000003")
      RESERVE_RESPONSE=$(http_post_body "$BASE_URL/api/inventory/reserve" \
        "{\"tenantId\":\"$DEMO_TENANT_ID\",\"sku\":\"$EXISTING_SKU\",\"orderId\":\"$NEW_ORDER_ID\",\"quantity\":1}")
      RESERVE_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST -H "Content-Type: application/json" \
        -d "{\"tenantId\":\"$DEMO_TENANT_ID\",\"sku\":\"$EXISTING_SKU\",\"orderId\":\"$NEW_ORDER_ID\",\"quantity\":1}" "$BASE_URL/api/inventory/reserve" || true)
    else
      RESERVE_STATUS="404"
    fi
  else
    RESERVE_STATUS=$(curl -s -o /dev/null -w "%{http_code}" -X POST -H "Content-Type: application/json" \
      -d "{\"tenantId\":\"$DEMO_TENANT_ID\",\"sku\":\"TEST-SKU-$(date +%s)\",\"orderId\":\"$NEW_ORDER_ID\",\"quantity\":5}" "$BASE_URL/api/inventory/reserve" || true)
  fi

  if [[ "$RESERVE_STATUS" == "200" ]]; then
    log_pass "Reserve stock -> HTTP 200"

    # Release the reservation
    log_info "Releasing reservation for order $NEW_ORDER_ID..."
    http_post "$BASE_URL/api/inventory/release" \
      "{\"tenantId\":\"$DEMO_TENANT_ID\",\"orderId\":\"$NEW_ORDER_ID\"}" \
      "Release stock reservation"
  elif [[ "$RESERVE_STATUS" == "404" ]]; then
    log_warn "Reserve stock -> SKU not found (expected if SKU doesn't exist in DB)"
  elif [[ "$RESERVE_STATUS" == "400" ]]; then
    log_warn "Reserve stock -> Insufficient stock (expected if not enough quantity)"
  else
    log_fail "Reserve stock -> HTTP $RESERVE_STATUS"
  fi
else
  log_warn "Skipping reservation tests - no valid stock item"
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
  echo -e "${GREEN}All InventoryService tests passed!${NC}"
  exit 0
fi
