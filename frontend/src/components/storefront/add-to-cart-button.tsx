'use client';

import { useState } from 'react';
import { useCart } from '@/lib/store/cart';
import { Button } from '@/components/ui/button';
import { ShoppingCart, Check } from 'lucide-react';
import type { Product, ProductVariant } from '@/lib/types';

interface AddToCartButtonProps {
  product: Product;
  variant?: ProductVariant;
}

function getPlaceholderImage(id: string, name: string): string {
  const encoded = encodeURIComponent(name.substring(0, 20));
  return `https://placehold.co/400x500/C8956C/FFFFFF?text=${encoded}`;
}

export function AddToCartButton({ product, variant }: AddToCartButtonProps) {
  const [added, setAdded] = useState(false);
  const addItem = useCart((state) => state.addItem);

  const handleAdd = () => {
    const price = variant?.priceOverride ?? product.basePrice;
    addItem({
      productId: product.id,
      variantId: variant?.id,
      name: product.name,
      variantName: variant?.name,
      price,
      image: getPlaceholderImage(product.id, product.name),
    });
    setAdded(true);
    setTimeout(() => setAdded(false), 2000);
  };

  return (
    <Button onClick={handleAdd} className="w-full sm:w-auto" size="lg">
      {added ? (
        <Check className="mr-2 h-4 w-4" />
      ) : (
        <ShoppingCart className="mr-2 h-4 w-4" />
      )}
      {added ? 'Added to Cart' : 'Add to Cart'}
    </Button>
  );
}
