import Image from 'next/image';
import Link from 'next/link';
import { Badge } from '@/components/ui/badge';
import type { Product } from '@/lib/types';

interface ProductCardProps {
  product: Product;
  tenantSlug: string;
}

function getPlaceholderImage(id: string, name: string): string {
  const encoded = encodeURIComponent(name.substring(0, 20));
  return `https://placehold.co/400x500/C8956C/FFFFFF?text=${encoded}`;
}

export function ProductCard({ product, tenantSlug }: ProductCardProps) {
  const imageUrl = getPlaceholderImage(product.id, product.name);
  const hasSale = product.salePrice !== undefined && product.salePrice !== null && product.salePrice < product.basePrice;

  return (
    <Link href={`/${tenantSlug}/products/${product.id}`} className="group block">
      <article className="overflow-hidden rounded-xl bg-card shadow-sm ring-1 ring-foreground/10 transition-shadow hover:shadow-md">
        <div className="relative aspect-[4/5] overflow-hidden bg-muted">
          <Image
            src={imageUrl}
            alt={product.name}
            fill
            className="object-cover transition-transform duration-300 group-hover:scale-105"
            sizes="(max-width: 640px) 50vw, (max-width: 1024px) 33vw, 25vw"
          />
          {hasSale && (
            <Badge className="absolute left-3 top-3 bg-primary text-primary-foreground">
              Sale
            </Badge>
          )}
        </div>
        <div className="p-4">
          <p className="text-xs font-medium uppercase tracking-widest text-muted-foreground">
            {product.category?.name ?? 'Uncategorized'}
          </p>
          <h3 className="mt-1 font-medium text-foreground line-clamp-2">{product.name}</h3>
          <p className="mt-1 text-sm text-muted-foreground">{product.sku}</p>
          <div className="mt-2 flex items-center gap-2">
            <span className="font-semibold text-foreground">
              ${hasSale ? product.salePrice!.toFixed(2) : product.basePrice.toFixed(2)}
            </span>
            {hasSale && (
              <span className="text-sm text-muted-foreground line-through">
                ${product.basePrice.toFixed(2)}
              </span>
            )}
          </div>
        </div>
      </article>
    </Link>
  );
}
