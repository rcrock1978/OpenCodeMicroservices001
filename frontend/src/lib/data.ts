export interface Product {
  id: string;
  name: string;
  slug: string;
  description: string;
  price: number;
  originalPrice?: number;
  category: string;
  brand: string;
  image: string;
  rating: number;
  reviewCount: number;
  badge?: string;
}

export const mockProducts: Product[] = [
  {
    id: 'prod-1',
    name: 'Linen Blazer',
    slug: 'linen-blazer',
    description:
      'A timeless linen blazer tailored for warm-weather sophistication. Lightweight, breathable, and effortlessly sharp.',
    price: 129.0,
    originalPrice: 180.0,
    category: 'Outerwear',
    brand: 'Maison Edit',
    image: 'https://placehold.co/400x500/C8956C/FFFFFF?text=Linen+Blazer',
    rating: 4.5,
    reviewCount: 124,
    badge: 'Sale',
  },
  {
    id: 'prod-2',
    name: 'Merino Crew Neck',
    slug: 'merino-crew-neck',
    description:
      'Ultra-soft merino wool crew neck sweater with a relaxed silhouette. Perfect for layering year-round.',
    price: 89.0,
    category: 'Tops',
    brand: 'Nordic Basics',
    image: 'https://placehold.co/400x500/4A4540/FFFFFF?text=Merino+Crew',
    rating: 4.8,
    reviewCount: 89,
  },
  {
    id: 'prod-3',
    name: 'Cotton Tapered Trousers',
    slug: 'cotton-tapered-trousers',
    description:
      'Tailored cotton trousers with a modern tapered leg. Features hidden stretch for all-day comfort.',
    price: 110.0,
    category: 'Bottoms',
    brand: 'Atelier Standard',
    image: 'https://placehold.co/400x500/8A8480/FFFFFF?text=Tapered+Trousers',
    rating: 4.3,
    reviewCount: 56,
    badge: 'New',
  },
  {
    id: 'prod-4',
    name: 'Silk Scarf',
    slug: 'silk-scarf',
    description:
      'Hand-finished silk scarf with an abstract terracotta print. A versatile accent piece for any wardrobe.',
    price: 65.0,
    category: 'Accessories',
    brand: 'Maison Edit',
    image: 'https://placehold.co/400x500/F5EDE6/1A1714?text=Silk+Scarf',
    rating: 4.7,
    reviewCount: 42,
  },
  {
    id: 'prod-5',
    name: 'Leather Crossbody',
    slug: 'leather-crossbody',
    description:
      'Compact vegetable-tanned leather crossbody bag with brass hardware. Designed to age beautifully.',
    price: 245.0,
    category: 'Bags',
    brand: 'Atelier Standard',
    image: 'https://placehold.co/400x500/2D7A4F/FFFFFF?text=Crossbody',
    rating: 4.9,
    reviewCount: 210,
    badge: 'Bestseller',
  },
  {
    id: 'prod-6',
    name: 'Linen Shirt Dress',
    slug: 'linen-shirt-dress',
    description:
      'A breezy shirt dress in premium linen with mother-of-pearl buttons and a self-tie waist.',
    price: 155.0,
    originalPrice: 195.0,
    category: 'Dresses',
    brand: 'Maison Edit',
    image: 'https://placehold.co/400x500/C8956C/FFFFFF?text=Shirt+Dress',
    rating: 4.6,
    reviewCount: 78,
    badge: 'Sale',
  },
  {
    id: 'prod-7',
    name: 'Cashmere Beanie',
    slug: 'cashmere-beanie',
    description:
      'Ribbed cashmere beanie with a folded brim. Lightweight warmth for crisp mornings.',
    price: 75.0,
    category: 'Accessories',
    brand: 'Nordic Basics',
    image: 'https://placehold.co/400x500/4A4540/FFFFFF?text=Cashmere+Beanie',
    rating: 4.4,
    reviewCount: 33,
  },
  {
    id: 'prod-8',
    name: 'Structured Tote',
    slug: 'structured-tote',
    description:
      'Oversized structured tote in grain leather with an internal laptop sleeve and magnetic closure.',
    price: 320.0,
    category: 'Bags',
    brand: 'Atelier Standard',
    image: 'https://placehold.co/400x500/1A1714/FFFFFF?text=Structured+Tote',
    rating: 4.8,
    reviewCount: 156,
    badge: 'New',
  },
];
