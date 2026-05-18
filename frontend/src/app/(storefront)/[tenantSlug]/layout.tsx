import Link from 'next/link';
import { ShoppingCart } from 'lucide-react';
import { getTenantBySubdomain } from '@/lib/api';

export default async function StorefrontLayout({
  children,
  params,
}: {
  children: React.ReactNode;
  params: Promise<{ tenantSlug: string }>;
}) {
  const { tenantSlug } = await params;
  const tenantRes = await getTenantBySubdomain(tenantSlug);
  const tenantName = tenantRes.data?.name ?? tenantSlug.charAt(0).toUpperCase() + tenantSlug.slice(1);

  return (
    <div className="flex min-h-full flex-col">
      <header className="sticky top-0 z-50 border-b bg-background/95 backdrop-blur">
        <div className="mx-auto flex h-16 max-w-7xl items-center justify-between px-4 sm:px-6 lg:px-8">
          <Link
            href={`/${tenantSlug}`}
            className="font-heading text-xl font-semibold tracking-tight"
          >
            {tenantName}
          </Link>
          <nav className="flex items-center gap-6">
            <Link
              href={`/${tenantSlug}`}
              className="text-sm font-medium text-foreground hover:text-primary"
            >
              Shop
            </Link>
            <Link
              href={`/${tenantSlug}/cart`}
              className="relative text-sm font-medium text-foreground hover:text-primary"
            >
              <ShoppingCart className="h-5 w-5" />
              <span className="sr-only">Cart</span>
            </Link>
          </nav>
        </div>
      </header>
      <main className="flex-1">{children}</main>
      <footer className="border-t bg-background py-8 text-center text-sm text-muted-foreground">
        <p>© 2026 {tenantName}. All rights reserved.</p>
      </footer>
    </div>
  );
}
