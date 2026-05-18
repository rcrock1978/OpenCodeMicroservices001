import Image from 'next/image';
import Link from 'next/link';
import { getProductById } from '@/lib/api';
import { AddToCartButton } from '@/components/storefront/add-to-cart-button';
import { Badge } from '@/components/ui/badge';

function getPlaceholderImage(id: string, name: string): string {
  const encoded = encodeURIComponent(name.substring(0, 20));
  return `https://placehold.co/400x500/C8956C/FFFFFF?text=${encoded}`;
}

export default async function ProductDetailPage({
  params,
}: {
  params: Promise<{ tenantSlug: string; id: string }>;
}) {
  const { tenantSlug, id } = await params;
  const productRes = await getProductById(id);
  const product = productRes.data;

  if (!product) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-16 text-center sm:px-6 lg:px-8">
        <h1 className="font-heading text-2xl font-semibold">Product not found</h1>
        <Link
          href={`/${tenantSlug}`}
          className="mt-4 inline-block text-primary hover:underline"
        >
          Back to shop
        </Link>
      </div>
    );
  }

  const imageUrl = getPlaceholderImage(product.id, product.name);
  const hasSale = product.salePrice !== undefined && product.salePrice !== null && product.salePrice < product.basePrice;

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="grid gap-8 lg:grid-cols-2 lg:gap-12">
        <div className="relative aspect-square overflow-hidden rounded-xl bg-muted lg:aspect-[4/5]">
          <Image
            src={imageUrl}
            alt={product.name}
            fill
            className="object-cover"
            sizes="(max-width: 1024px) 100vw, 50vw"
            priority
          />
          {hasSale && (
            <Badge className="absolute left-4 top-4 bg-primary text-primary-foreground">
              Sale
            </Badge>
          )}
        </div>
        <div className="flex flex-col">
          <p className="text-xs font-medium uppercase tracking-widest text-muted-foreground">
            {product.category?.name ?? 'Uncategorized'}
          </p>
          <h1 className="mt-2 font-heading text-3xl font-semibold tracking-tight">
            {product.name}
          </h1>
          <p className="mt-1 text-sm text-muted-foreground">{product.sku}</p>
          <div className="mt-4 flex items-center gap-3">
            <span className="text-2xl font-semibold text-foreground">
              ${hasSale ? product.salePrice!.toFixed(2) : product.basePrice.toFixed(2)}
            </span>
            {hasSale && (
              <span className="text-lg text-muted-foreground line-through">
                ${product.basePrice.toFixed(2)}
              </span>
            )}
          </div>
          <p className="mt-6 leading-relaxed text-muted-foreground">{product.description}</p>

          {product.variants.length > 0 && (
            <div className="mt-6">
              <h3 className="text-sm font-medium text-foreground">Variants</h3>
              <div className="mt-2 flex flex-wrap gap-2">
                {product.variants.map((variant) => (
                  <Badge key={variant.id} variant="outline">
                    {variant.name} — {variant.priceOverride ? `$${variant.priceOverride.toFixed(2)}` : 'Same price'}
                  </Badge>
                ))}
              </div>
            </div>
          )}

          <div className="mt-8 flex flex-col gap-4 sm:flex-row sm:items-center">
            <AddToCartButton product={product} />
            <Link
              href={`/${tenantSlug}`}
              className="text-center text-sm font-medium text-muted-foreground hover:text-foreground sm:text-left"
            >
              Continue Shopping
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
