import { mockProducts } from '@/lib/data';
import { ProductCard } from '@/components/storefront/product-card';

export default async function TenantHomePage({
  params,
}: {
  params: Promise<{ tenantSlug: string }>;
}) {
  const { tenantSlug } = await params;

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <section className="mb-12 rounded-2xl bg-secondary p-8 sm:p-12">
        <h1 className="font-heading text-3xl font-semibold tracking-tight sm:text-4xl">
          Welcome to {tenantSlug.charAt(0).toUpperCase() + tenantSlug.slice(1)}
        </h1>
        <p className="mt-4 max-w-xl text-muted-foreground">
          Discover curated collections designed with intention. Each piece is chosen for quality,
          sustainability, and timeless style.
        </p>
      </section>

      <h2 className="mb-6 font-heading text-2xl font-semibold">Featured Products</h2>
      <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-4">
        {mockProducts.map((product) => (
          <ProductCard key={product.id} product={product} tenantSlug={tenantSlug} />
        ))}
      </div>
    </div>
  );
}
