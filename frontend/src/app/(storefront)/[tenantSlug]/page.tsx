import { getTenantBySubdomain, getProductsByTenant } from '@/lib/api';
import { ProductCard } from '@/components/storefront/product-card';

export default async function TenantHomePage({
  params,
}: {
  params: Promise<{ tenantSlug: string }>;
}) {
  const { tenantSlug } = await params;

  const tenantRes = await getTenantBySubdomain(tenantSlug);
  const tenant = tenantRes.data;

  let products: Awaited<ReturnType<typeof getProductsByTenant>>['data'] = [];
  if (tenant) {
    const productsRes = await getProductsByTenant(tenant.id);
    products = productsRes.data ?? [];
  }

  const tenantName = tenant?.name ?? tenantSlug.charAt(0).toUpperCase() + tenantSlug.slice(1);

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <section className="mb-12 rounded-2xl bg-secondary p-8 sm:p-12">
        <h1 className="font-heading text-3xl font-semibold tracking-tight sm:text-4xl">
          Welcome to {tenantName}
        </h1>
        <p className="mt-4 max-w-xl text-muted-foreground">
          Discover curated collections designed with intention. Each piece is chosen for quality,
          sustainability, and timeless style.
        </p>
      </section>

      <h2 className="mb-6 font-heading text-2xl font-semibold">Featured Products</h2>
      {products && products.length > 0 ? (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
          {products.map((product) => (
            <ProductCard key={product.id} product={product} tenantSlug={tenantSlug} />
          ))}
        </div>
      ) : (
        <p className="text-muted-foreground">No products available at the moment.</p>
      )}
    </div>
  );
}
