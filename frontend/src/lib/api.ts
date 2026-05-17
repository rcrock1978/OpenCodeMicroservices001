const API_BASE = process.env.API_BASE_URL || 'http://localhost:5050';

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

export async function get<T>(path: string, init?: RequestInit): Promise<ApiResponse<T>> {
  const res = await fetch(`${API_BASE}${path}`, {
    ...init,
    method: 'GET',
    headers: {
      'Content-Type': 'application/json',
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
}

export async function post<T>(path: string, body: unknown, init?: RequestInit): Promise<ApiResponse<T>> {
  const res = await fetch(`${API_BASE}${path}`, {
    ...init,
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
      ...(init?.headers || {}),
    },
    body: JSON.stringify(body),
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
}
