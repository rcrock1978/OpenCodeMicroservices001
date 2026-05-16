import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: 'standalone',
  images: {
    minimumCacheTTL: 60 * 60 * 24,
    deviceSizes: [640, 750, 1080, 1920],
  },
};

export default nextConfig;
