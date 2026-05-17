import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";

const services = [
  { name: "API Gateway", status: "Operational", url: "http://localhost:5050" },
  { name: "Identity Service", status: "Operational", url: "http://localhost:5001" },
  { name: "Billing Service", status: "Operational", url: "http://localhost:5002" },
  { name: "Notification Service", status: "Operational", url: "http://localhost:5003" },
  { name: "Core Service", status: "Operational", url: "http://localhost:5004" },
];

export default function Home() {
  return (
    <div className="min-h-full bg-muted/40">
      <header className="border-b bg-background">
        <div className="mx-auto flex max-w-7xl items-center justify-between px-4 py-4 sm:px-6 lg:px-8">
          <div>
            <h1 className="text-2xl font-bold tracking-tight">SaaS Platform</h1>
            <p className="text-sm text-muted-foreground">Multi-tenant microservices dashboard</p>
          </div>
          <Badge variant="secondary">v0.1.0</Badge>
        </div>
      </header>

      <main className="mx-auto max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
          <Card>
            <CardHeader>
              <CardTitle>Tenants</CardTitle>
              <CardDescription>Active tenant organizations</CardDescription>
            </CardHeader>
            <CardContent>
              <p className="text-3xl font-bold">0</p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Users</CardTitle>
              <CardDescription>Registered user accounts</CardDescription>
            </CardHeader>
            <CardContent>
              <p className="text-3xl font-bold">0</p>
            </CardContent>
          </Card>

          <Card>
            <CardHeader>
              <CardTitle>Subscriptions</CardTitle>
              <CardDescription>Active paid subscriptions</CardDescription>
            </CardHeader>
            <CardContent>
              <p className="text-3xl font-bold">0</p>
            </CardContent>
          </Card>
        </div>

        <div className="mt-8">
          <h2 className="text-lg font-semibold mb-4">Service Status</h2>
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {services.map((service) => (
              <Card key={service.name}>
                <CardHeader className="pb-2">
                  <div className="flex items-center justify-between">
                    <CardTitle className="text-base">{service.name}</CardTitle>
                    <Badge variant="outline" className="text-green-600 border-green-200 bg-green-50">
                      {service.status}
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent>
                  <p className="text-xs text-muted-foreground">{service.url}</p>
                </CardContent>
              </Card>
            ))}
          </div>
        </div>

        <div className="mt-8 rounded-lg border bg-card p-6">
          <h2 className="text-lg font-semibold mb-2">Getting Started</h2>
          <ul className="list-disc list-inside space-y-1 text-sm text-muted-foreground">
            <li>Install .NET 10 SDK to build and run backend services</li>
            <li>Install Docker to run infrastructure dependencies (Postgres, Redis, RabbitMQ, Jaeger)</li>
            <li>Run <code className="rounded bg-muted px-1 py-0.5 text-xs font-mono">docker-compose up -d</code> to start infrastructure</li>
            <li>Run <code className="rounded bg-muted px-1 py-0.5 text-xs font-mono">npm run dev</code> in the frontend directory</li>
            <li>Open <code className="rounded bg-muted px-1 py-0.5 text-xs font-mono">http://localhost:3000</code> to view the dashboard</li>
          </ul>
        </div>
      </main>
    </div>
  );
}
