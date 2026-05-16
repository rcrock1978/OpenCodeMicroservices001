import Image from 'next/image';
import Link from 'next/link';
import { mockProducts } from '@/lib/data';
import { AddToCartButton } from '@/components/storefront/add-to-cart-button';
import { Badge } from '@/components/ui/badge';
import { Star } from 'lucide-react';

export default async function ProductDetailPage({
  params,
}: {
  params: Promise<{ tenantSlug: string; id: string }>;
}) {
  const { tenantSlug, id } = await params;
  const product = mockProducts.find((p) => p.id === id);

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

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <div className="grid gap-8 lg:grid-cols-2 lg:gap-12">
        <div className="relative aspect-square overflow-hidden rounded-xl bg-muted lg:aspect-[4/5]">
          <Image
            src={product.image}
            alt={product.name}
            fill
            className="object-cover"
            sizes="(max-width: 1024px) 100vw, 50vw"
            priority
          />
          {product.badge && (
            <Badge className="absolute left-4 top-4 bg-primary text-primary-foreground">
              {product.badge}
            </Badge>
          )}
        </div>
        <div className="flex flex-col">
          <p className="text-xs font-medium uppercase tracking-widest text-muted-foreground">
            {product.category}
          </p>
          <h1 className="mt-2 font-heading text-3xl font-semibold tracking-tight">
            {product.name}
          </h1>
          <div className="mt-2 flex items-center gap-2">
            <div className="flex items-center text-yellow-500">
              <Star className="h-4 w-4 fill-current" />
              <span className="ml-1 text-sm font-medium text-foreground">
                {product.rating}
              </span>
            </div>
            <span className="text-sm text-muted-foreground">
              ({product.reviewCount} reviews)
            </span>
          </div>
          <div className="mt-4 flex items-center gap-3">
            <span className="text-2xl font-semibold text-foreground">
              ${product.price.toFixed(2)}
            </span>
            {product.originalPrice && (
              <span className="text-lg text-muted-foreground line-through">
                ${product.originalPrice.toFixed(2)}
              </span>
            )}
          </div>
          <p className="mt-6 leading-relaxed text-muted-foreground">{product.description}</p>
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
