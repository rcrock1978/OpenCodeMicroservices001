import type {
  Tenant,
  User,
  Product,
  Category,
  Order,
  Customer,
} from './types';

const API_BASE = process.env.API_BASE_URL || 'http://localhost:5000';

export interface ApiError {
  code: string;
  message: string;
  field?: string;
}

export interface ApiResponse<T> {
  data: T | null;
  error: ApiError | null;
  meta: {
    requestId: string;
    timestamp: string;
    pagination?: {
      page: number;
      perPage: number;
      total: number;
      totalPages: number;
    };
  };
}

async function request<T>(
  path: string,
  init?: RequestInit
): Promise<ApiResponse<T>> {
  const url = `${API_BASE}${path}`;
  try {
    const res = await fetch(url, {
      ...init,
      headers: {
        'Content-Type': 'application/json',
        'X-Correlation-ID': crypto.randomUUID(),
        ...(init?.headers || {}),
      },
    });

    const json = await res.json().catch(() => null);
    if (!res.ok) {
      return {
        data: null,
        error: json?.error || { code: `HTTP_${res.status}`, message: res.statusText },
        meta: json?.meta || { requestId: '', timestamp: new Date().toISOString() },
      };
    }
    return json as ApiResponse<T>;
  } catch {
    return {
      data: null,
      error: { code: 'NETWORK_ERROR', message: 'Failed to reach the API gateway' },
      meta: { requestId: crypto.randomUUID(), timestamp: new Date().toISOString() },
    };
  }
}

export async function get<T>(path: string, init?: RequestInit): Promise<ApiResponse<T>> {
  return request<T>(path, { ...init, method: 'GET' });
}

export async function post<T>(path: string, body: unknown, init?: RequestInit): Promise<ApiResponse<T>> {
  return request<T>(path, {
    ...init,
    method: 'POST',
    body: JSON.stringify(body),
  });
}

export async function put<T>(path: string, body: unknown, init?: RequestInit): Promise<ApiResponse<T>> {
  return request<T>(path, {
    ...init,
    method: 'PUT',
    body: JSON.stringify(body),
  });
}

export async function del<T>(path: string, init?: RequestInit): Promise<ApiResponse<T>> {
  return request<T>(path, { ...init, method: 'DELETE' });
}

// --- Identity API ---

export function getTenants() {
  return get<Tenant[]>('/api/identity/tenants');
}

export function getTenantById(id: string) {
  return get<Tenant>(`/api/identity/tenants/${id}`);
}

export function getTenantBySubdomain(subdomain: string) {
  return get<Tenant>(`/api/identity/tenants/by-subdomain/${subdomain}`);
}

export function getUsers() {
  return get<User[]>('/api/identity/users');
}

// --- Catalog API ---

export function getProducts() {
  return get<Product[]>('/api/catalog/products');
}

export function getProductsByTenant(tenantId: string) {
  return get<Product[]>(`/api/catalog/products/tenant/${tenantId}`);
}

export function getProductById(id: string) {
  return get<Product>(`/api/catalog/products/${id}`);
}

export function getCategories() {
  return get<Category[]>('/api/catalog/categories');
}

// --- Order API ---

export function getOrders() {
  return get<Order[]>('/api/orders');
}

export function getOrderById(id: string) {
  return get<Order>(`/api/orders/${id}`);
}

// --- Customer API ---

export function getCustomers() {
  return get<Customer[]>('/api/customers');
}

export function getCustomerById(id: string) {
  return get<Customer>(`/api/customers/${id}`);
}
