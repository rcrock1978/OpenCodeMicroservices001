'use client';

import { useState } from 'react';
import { useCart } from '@/lib/store/cart';
import { Button } from '@/components/ui/button';
import { ShoppingCart, Check } from 'lucide-react';

interface AddToCartButtonProps {
  product: {
    id: string;
    name: string;
    price: number;
    image: string;
  };
}

export function AddToCartButton({ product }: AddToCartButtonProps) {
  const [added, setAdded] = useState(false);
  const addItem = useCart((state) => state.addItem);

  const handleAdd = () => {
    addItem({
      productId: product.id,
      name: product.name,
      price: product.price,
      image: product.image,
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
