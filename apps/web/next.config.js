/** @type {import('next').NextConfig} */
const nextConfig = {
  async rewrites() {
    // Use INTERNAL_API_URL if defined (e.g. in Docker), otherwise default to localhost for npm run dev
    const apiGatewayUrl = process.env.INTERNAL_API_URL || "http://localhost:5000";

    return [
      {
        source: "/gateway-users/:path*",
        destination: `${apiGatewayUrl}/gateway-users/:path*`,
      },
      {
        source: "/gateway-accounts/:path*",
        destination: `${apiGatewayUrl}/gateway-accounts/:path*`,
      },
      {
        source: "/gateway-transactions/:path*",
        destination: `${apiGatewayUrl}/gateway-transactions/:path*`,
      },
      {
        source: "/gateway-obligations/:path*",
        destination: `${apiGatewayUrl}/gateway-obligations/:path*`,
      },
    ];
  },
};

export default nextConfig;
