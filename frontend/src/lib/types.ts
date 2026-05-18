/**
 * Frontend type definitions aligned with backend DTOs.
 */

// --- Identity Service ---

export interface Tenant {
  id: string;
  name: string;
  subdomain: string;
  subscriptionPlanId?: string;
  createdAt: string;
  isActive: boolean;
}

export interface TenantSummary {
  id: string;
  name: string;
  subdomain: string;
  subscriptionPlanId?: string;
  createdAt: string;
  isActive: boolean;
}

export type UserRole = 'Member' | 'Admin' | 'Owner';

export interface User {
  id: string;
  email: string;
  displayName: string;
  tenantId: string;
  tenant: TenantSummary | null;
  role: UserRole;
  createdAt: string;
  isActive: boolean;
}

// --- Catalog Service ---

export interface CategorySummary {
  id: string;
  tenantId: string;
  name: string;
  parentCategoryId?: string;
  isActive: boolean;
}

export interface Category {
  id: string;
  tenantId: string;
  name: string;
  parentCategoryId?: string;
  isActive: boolean;
}

export interface ProductVariant {
  id: string;
  productId: string;
  name: string;
  sku: string;
  priceOverride?: number;
  attributes?: string;
}

export interface Product {
  id: string;
  tenantId: string;
  name: string;
  description?: string;
  sku: string;
  basePrice: number;
  salePrice?: number;
  currency: string;
  categoryId: string;
  category: CategorySummary | null;
  isActive: boolean;
  createdAt: string;
  variants: ProductVariant[];
}

// --- Order Service ---

export type OrderStatus =
  | 'Pending'
  | 'Confirmed'
  | 'PaymentPending'
  | 'Paid'
  | 'Shipped'
  | 'Delivered'
  | 'Cancelled'
  | 'Refunded';

export interface OrderItem {
  id: string;
  productId: string;
  productVariantId: string;
  productName: string;
  sku: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface Order {
  id: string;
  tenantId: string;
  customerId: string;
  orderNumber: string;
  status: OrderStatus;
  subtotal: number;
  shippingCost: number;
  taxAmount: number;
  total: number;
  currency: string;
  shippingAddress?: string;
  createdAt: string;
  items: OrderItem[];
}

// --- Customer Service ---

export type AddressType = 'Shipping' | 'Billing';

export interface Address {
  id: string;
  customerId: string;
  type: AddressType;
  street: string;
  city: string;
  state?: string;
  postalCode: string;
  country: string;
  isDefault: boolean;
}

export interface Customer {
  id: string;
  tenantId: string;
  userId?: string;
  email: string;
  firstName: string;
  lastName: string;
  phoneNumber?: string;
  createdAt: string;
  addresses: Address[];
}

// --- Cart (client-side only) ---

export interface CartItem {
  productId: string;
  variantId?: string;
  name: string;
  variantName?: string;
  price: number;
  image?: string;
  quantity: number;
}
