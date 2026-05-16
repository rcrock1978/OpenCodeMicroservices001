'use client';

import { use } from 'react';
import Image from 'next/image';
import Link from 'next/link';
import { useCart } from '@/lib/store/cart';
import { Button } from '@/components/ui/button';
import { Minus, Plus, Trash2, ShoppingBag } from 'lucide-react';

export default function CartPage({
  params,
}: {
  params: Promise<{ tenantSlug: string }>;
}) {
  const { tenantSlug } = use(params);
  const items = useCart((state) => state.items);
  const updateQuantity = useCart((state) => state.updateQuantity);
  const removeItem = useCart((state) => state.removeItem);
  const totalPrice = useCart((state) => state.totalPrice);

  if (items.length === 0) {
    return (
      <div className="mx-auto max-w-7xl px-4 py-16 text-center sm:px-6 lg:px-8">
        <ShoppingBag className="mx-auto h-12 w-12 text-muted-foreground" />
        <h1 className="mt-4 font-heading text-2xl font-semibold">Your cart is empty</h1>
        <p className="mt-2 text-muted-foreground">
          Looks like you have not added anything yet.
        </p>
        <Link href={`/${tenantSlug}`}>
          <Button className="mt-6">Start Shopping</Button>
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
      <h1 className="font-heading text-2xl font-semibold">Your Cart</h1>
      <div className="mt-8 grid gap-8 lg:grid-cols-3">
        <div className="space-y-4 lg:col-span-2">
          {items.map((item) => (
            <div
              key={item.productId}
              className="flex gap-4 rounded-xl border bg-card p-4"
            >
              <div className="relative h-24 w-24 shrink-0 overflow-hidden rounded-lg bg-muted">
                <Image
                  src={item.image}
                  alt={item.name}
                  fill
                  className="object-cover"
                  sizes="96px"
                />
              </div>
              <div className="flex flex-1 flex-col justify-between">
                <div>
                  <h3 className="font-medium">{item.name}</h3>
                  <p className="text-sm text-muted-foreground">
                    ${item.price.toFixed(2)}
                  </p>
                </div>
                <div className="flex items-center justify-between">
                  <div className="flex items-center gap-2">
                    <button
                      onClick={() =>
                        updateQuantity(item.productId, item.quantity - 1)
                      }
                      className="inline-flex h-8 w-8 items-center justify-center rounded-md border hover:bg-muted"
                      aria-label="Decrease quantity"
                    >
                      <Minus className="h-3 w-3" />
                    </button>
                    <span className="w-8 text-center text-sm font-medium">
                      {item.quantity}
                    </span>
                    <button
                      onClick={() =>
                        updateQuantity(item.productId, item.quantity + 1)
                      }
                      className="inline-flex h-8 w-8 items-center justify-center rounded-md border hover:bg-muted"
                      aria-label="Increase quantity"
                    >
                      <Plus className="h-3 w-3" />
                    </button>
                  </div>
                  <button
                    onClick={() => removeItem(item.productId)}
                    className="text-muted-foreground hover:text-destructive"
                    aria-label="Remove item"
                  >
                    <Trash2 className="h-4 w-4" />
                  </button>
                </div>
              </div>
            </div>
          ))}
        </div>
        <div className="rounded-xl border bg-card p-6">
          <h2 className="font-heading text-lg font-semibold">Order Summary</h2>
          <div className="mt-4 space-y-2">
            <div className="flex justify-between text-sm">
              <span className="text-muted-foreground">Subtotal</span>
              <span className="font-medium">${totalPrice().toFixed(2)}</span>
            </div>
            <div className="flex justify-between text-sm">
              <span className="text-muted-foreground">Shipping</span>
              <span className="font-medium">FREE</span>
            </div>
            <div className="flex justify-between border-t pt-2">
              <span className="font-medium">Total</span>
              <span className="font-semibold">${totalPrice().toFixed(2)}</span>
            </div>
          </div>
          <Button className="mt-6 w-full" size="lg">
            Checkout
          </Button>
          <Link
            href={`/${tenantSlug}`}
            className="mt-3 block text-center text-sm text-muted-foreground hover:text-foreground"
          >
            Continue Shopping
          </Link>
        </div>
      </div>
    </div>
  );
}
