export async function GET(request: Request) {
  const { searchParams } = new URL(request.url);
  const backendUrl = `http://localhost:5000/api/catalog/products?${searchParams.toString()}`;

  try {
    const res = await fetch(backendUrl, {
      headers: {
        'Content-Type': 'application/json',
        'X-Correlation-ID': crypto.randomUUID(),
      },
      cache: 'no-store',
    });

    const data = await res.json().catch(() => ({ error: 'Invalid JSON from backend' }));
    return Response.json(data, { status: res.status });
  } catch {
    return Response.json(
      {
        data: null,
        error: { code: 'PROXY_ERROR', message: 'Failed to reach catalog service' },
        meta: {
          requestId: crypto.randomUUID(),
          timestamp: new Date().toISOString(),
        },
      },
      { status: 502 }
    );
  }
}
