#!/usr/bin/env bash
#
# CatalogService Endpoint Test Script
# Tests all API endpoints for the Catalog Service directly
#
# Usage:
#   chmod +x test-catalogservice.sh
#   ./test-catalogservice.sh
#
# Prerequisites:
#   - CatalogService must be running (default port 5002)
#   - curl and jq must be installed
#

set -euo pipefail

# ── Configuration ──────────────────────────────────────────────────────────────
BASE_URL="http://localhost:5002"
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

http_put() {
  local url="$1"
  local json="$2"
  local desc="${3:-$url}"
  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" -X PUT -H "Content-Type: application/json" -d "$json" "$url" || true)
  if [[ "$status" == "200" || "$status" == "201" || "$status" == "204" ]]; then
    log_pass "PUT $desc -> HTTP $status"
    return 0
  else
    log_fail "PUT $desc -> HTTP $status (expected 200/201/204)"
    return 1
  fi
}

http_delete() {
  local url="$1"
  local desc="${2:-$url}"
  local status
  status=$(curl -s -o /dev/null -w "%{http_code}" -X DELETE "$url" || true)
  if [[ "$status" == "200" || "$status" == "204" ]]; then
    log_pass "DELETE $desc -> HTTP $status"
    return 0
  else
    log_fail "DELETE $desc -> HTTP $status (expected 200/204)"
    return 1
  fi
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
echo "║          CATALOG SERVICE ENDPOINT TEST SUITE                         ║"
echo "║          Target: $BASE_URL                              ║"
echo "╚══════════════════════════════════════════════════════════════════════╝"
echo ""

# ── 0. Service Health ────────────────────────────────────────────────────────
log_step "Service Health"
http_get "$BASE_URL/" "Root endpoint"
http_get "$BASE_URL/health/live" "Health: live"
http_get "$BASE_URL/health/ready" "Health: ready"

# ── 1. Categories ──────────────────────────────────────────────────────────────
log_step "Category Endpoints"

# List categories
http_get "$BASE_URL/api/categories" "List all categories"
CATEGORY_LIST=$(http_get_body "$BASE_URL/api/categories")
CATEGORY_COUNT=$(echo "$CATEGORY_LIST" | jq 'length' 2>/dev/null || echo "0")
log_info "Found $CATEGORY_COUNT existing categories"

# Create a new category
log_info "Creating a test category..."
CREATE_CATEGORY_RESPONSE=$(http_post_body "$BASE_URL/api/categories" \
  "{\"tenantId\":\"$DEMO_TENANT_ID\",\"name\":\"Test Category $(date +%s)\",\"parentCategoryId\":null}")

NEW_CATEGORY_ID=$(echo "$CREATE_CATEGORY_RESPONSE" | jq -r '.id // empty' 2>/dev/null || true)
if [[ -n "$NEW_CATEGORY_ID" && "$NEW_CATEGORY_ID" != "null" ]]; then
  log_pass "Create category -> ID: $NEW_CATEGORY_ID"
else
  log_fail "Create category -> No ID returned"
  NEW_CATEGORY_ID=""
fi

# Verify category appears in list
if [[ -n "$NEW_CATEGORY_ID" ]]; then
  http_get "$BASE_URL/api/categories" "List categories after create"
fi

# ── 2. Products ──────────────────────────────────────────────────────────────
log_step "Product Endpoints"

# List all products
http_get "$BASE_URL/api/products" "List all products"
PRODUCT_LIST=$(http_get_body "$BASE_URL/api/products")
PRODUCT_COUNT=$(echo "$PRODUCT_LIST" | jq 'length' 2>/dev/null || echo "0")
log_info "Found $PRODUCT_COUNT existing products"

# Pick an existing category ID for product creation
if [[ -n "$NEW_CATEGORY_ID" ]]; then
  TEST_CATEGORY_ID="$NEW_CATEGORY_ID"
else
  TEST_CATEGORY_ID=$(echo "$CATEGORY_LIST" | jq -r '.[0].id // empty' 2>/dev/null || true)
fi

if [[ -z "$TEST_CATEGORY_ID" || "$TEST_CATEGORY_ID" == "null" ]]; then
  log_warn "No category ID available for product creation. Skipping product write tests."
  TEST_CATEGORY_ID=""
else
  log_info "Using category ID: $TEST_CATEGORY_ID"
fi

# List products by tenant
http_get "$BASE_URL/api/products/tenant/$DEMO_TENANT_ID" "List products by tenant"

# Get a specific product (first one from list)
EXISTING_PRODUCT_ID=$(echo "$PRODUCT_LIST" | jq -r '.[0].id // empty' 2>/dev/null || true)
if [[ -n "$EXISTING_PRODUCT_ID" && "$EXISTING_PRODUCT_ID" != "null" ]]; then
  http_get "$BASE_URL/api/products/$EXISTING_PRODUCT_ID" "Get product by ID ($EXISTING_PRODUCT_ID)"
else
  log_warn "No existing product to test GET by ID"
fi

# Test 404 for non-existent product
http_expect "GET" "$BASE_URL/api/products/00000000-0000-0000-0000-000000000000" "404" "Get non-existent product (expect 404)"

# Create a product
if [[ -n "$TEST_CATEGORY_ID" ]]; then
  log_info "Creating a test product..."
  CREATE_PRODUCT_RESPONSE=$(http_post_body "$BASE_URL/api/products" \
    "{\"tenantId\":\"$DEMO_TENANT_ID\",\"name\":\"Gateway Test Product\",\"description\":\"A product created by the test script\",\"sku\":\"TEST-SKU-$(date +%s)\",\"basePrice\":99.99,\"salePrice\":79.99,\"currency\":\"USD\",\"categoryId\":\"$TEST_CATEGORY_ID\"}")

  NEW_PRODUCT_ID=$(echo "$CREATE_PRODUCT_RESPONSE" | jq -r '.id // empty' 2>/dev/null || true)
  if [[ -n "$NEW_PRODUCT_ID" && "$NEW_PRODUCT_ID" != "null" ]]; then
    log_pass "Create product -> ID: $NEW_PRODUCT_ID"

    # Verify product exists
    http_get "$BASE_URL/api/products/$NEW_PRODUCT_ID" "Get created product"

    # Update the product
    http_put "$BASE_URL/api/products/$NEW_PRODUCT_ID" \
      "{\"name\":\"Updated Product Name\",\"description\":\"Updated description\",\"basePrice\":109.99,\"salePrice\":89.99,\"isActive\":true}" \
      "Update product"

    # Verify update
    UPDATED_PRODUCT=$(http_get_body "$BASE_URL/api/products/$NEW_PRODUCT_ID")
    UPDATED_NAME=$(echo "$UPDATED_PRODUCT" | jq -r '.name // empty' 2>/dev/null || true)
    if [[ "$UPDATED_NAME" == "Updated Product Name" ]]; then
      log_pass "Verify update -> name is 'Updated Product Name'"
    else
      log_fail "Verify update -> name is '$UPDATED_NAME' (expected 'Updated Product Name')"
    fi

    # Soft-delete the product
    http_delete "$BASE_URL/api/products/$NEW_PRODUCT_ID" "Soft-delete product"

    # Verify product still exists but may be inactive (soft delete)
    DELETE_VERIFY=$(http_get_body "$BASE_URL/api/products/$NEW_PRODUCT_ID")
    if [[ -n "$DELETE_VERIFY" && "$DELETE_VERIFY" != "null" ]]; then
      log_pass "Verify soft-delete -> product still retrievable"
    else
      log_warn "Verify soft-delete -> product not found (may be hard deleted or filtered)"
    fi
  else
    log_fail "Create product -> No ID returned"
  fi
else
  log_warn "Skipping product write tests - no valid category ID"
fi

# ── 3. Product by Tenant (with active filter) ──────────────────────────────
log_step "Tenant-scoped Product Queries"
TENANT_PRODUCTS=$(http_get_body "$BASE_URL/api/products/tenant/$DEMO_TENANT_ID")
TENANT_COUNT=$(echo "$TENANT_PRODUCTS" | jq 'length' 2>/dev/null || echo "0")
log_info "Tenant $DEMO_TENANT_ID has $TENANT_COUNT active products"

# Verify all returned products belong to the tenant and are active
INVALID_PRODUCTS=$(echo "$TENANT_PRODUCTS" | jq '[.[] | select(.tenantId != "'$DEMO_TENANT_ID'" or .isActive != true)] | length' 2>/dev/null || echo "0")
if [[ "$INVALID_PRODUCTS" == "0" ]]; then
  log_pass "Tenant filter -> all products belong to tenant and are active"
else
  log_fail "Tenant filter -> $INVALID_PRODUCTS products have wrong tenantId or are inactive"
fi

# ── 4. Category Cleanup ──────────────────────────────────────────────────────
if [[ -n "$NEW_CATEGORY_ID" && "$NEW_CATEGORY_ID" != "null" ]]; then
  log_step "Cleanup"
  log_info "Test category $NEW_CATEGORY_ID was created. Manual cleanup may be needed."
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
  echo -e "${GREEN}All CatalogService tests passed!${NC}"
  exit 0
fi
