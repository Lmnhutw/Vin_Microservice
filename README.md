<h1>VIN E-COMMERCE MICROSERVICE</h1>

<p>A comprehensive e-commerce platform built using .NET 8 with a modular architecture for handling various aspects of the shopping experience, from product management to checkout and order processing. This project includes microservices, integration with external services, and deployment on Azure.</p>

<h2>Table of Contents</h2>
<ol>
    <li><a href="#project-overview">Project Overview</a></li>
    <li><a href="#architecture">Architecture</a></li>
    <li><a href="#features">Features</a></li>
    <li><a href="#setup-instructions">Setup Instructions</a></li>
    <li><a href="#project-structure">Project Structure</a></li>
    <li><a href="#api-documentation">API Documentation</a></li>
    <li><a href="#deployment">Deployment</a></li>
    <li><a href="#future-enhancements">Future Enhancements</a></li>
</ol>

<h2 id="project-overview">Project Overview</h2>
<p>This platform is designed to simulate a full e-commerce system. It includes APIs for handling products, coupons, user authentication, shopping cart operations, order processing, and more. Key integrations include:</p>
<ul>
    <li><strong>Stripe</strong> for payment processing (Comming soon)</li>
    <li><strong>Azure</strong> for deployment and cloud services (Comming soon)</li>
    <li><strong>RabbitMQ</strong> and <strong>Azure Service Bus</strong> for communication between microservices (Comming soon)</li>
    <li><strong>SendGrid</strong> for email notifications (Working on)</li>
</ul>

<h2 id="architecture">Architecture</h2>
<p>The system is organized into several microservices, each responsible for specific functions such as product management, user authentication, and order processing. Communication between services is managed using message brokers like RabbitMQ and Azure Service Bus, ensuring scalability and fault tolerance.</p>

<h3>Services and Components</h3>
<ul>
    <li><strong>AuthAPI</strong>: Manages user registration, login, and role assignment.</li>
    <li><strong>ProductAPI</strong>: Handles product CRUD operations.</li>
    <li><strong>CartAPI</strong>: Manages the shopping cart and applies coupon codes.</li>
    <li><strong>OrderAPI</strong>: Processes orders and manages their status.</li>
    <li><strong>EmailAPI</strong>: Sends transactional emails such as order confirmations and password resets.</li>
    <li><strong>Gateway</strong>: Acts as an entry point for routing requests to appropriate services.</li>
    <li><strong>Client Application</strong>: A web front-end that interacts with APIs and manages the UI for end-users.</li>
</ul>

<h2 id="features">Features</h2>
<ol>
    <li><strong>User Authentication</strong> (AuthAPI)
        <ul>
            <li>Register, login, password reset, and role assignment</li>
            <li>Token-based authentication with JWT</li>
        </ul>
    </li>
    <li><strong>Product Management</strong> (ProductAPI) - CRUD operations for products</li>
    <li><strong>Coupon Management</strong> (CouponAPI) - CRUD for coupons and applying discounts</li>
    <li><strong>Shopping Cart</strong> (CartAPI and Cart Web Integration)
        <ul>
            <li>Add, update, and remove products in the cart</li>
            <li>Apply coupon codes for discounts</li>
        </ul>
    </li>
    <li><strong>Order Management</strong> (OrderAPI) - Checkout flow, order creation, and tracking. (Comming soon)</li>
    <li><strong>Payment Processing</strong> (Stripe Integration) - Secure checkout with Stripe. (Comming soon)</li>
    <li><strong>Email Notifications</strong> (EmailAPI) - Automated emails using SendGrid. (Working on)</li>
    <li><strong>Service Bus and Message Queueing</strong> - RabbitMQ and Azure Service Bus for async communication between services. (Working on Service Bus)</li>
    <li><strong>Azure Deployment</strong> - Full deployment on Azure, utilizing services like App Services, SQL Database, and Blob Storage. (Comming soon)</li>
</ol>

<h2 id="setup-instructions">Setup Instructions</h2>
<ol>
    <li><strong>Clone the repository</strong>:
        <pre><code>git clone &lt;repository-url&gt;</code></pre>
    </li>
    <li><strong>Install dependencies</strong>:
        <ul>
            <li>Ensure you have .NET 8 SDK installed.</li>
            <li>Install NuGet packages by restoring the solution.</li>
        </ul>
    </li>
    <li><strong>Configure Environment Variables</strong>:
        <ul>
            <li>Configure API keys and secrets for SendGrid, Stripe, and Azure in environment files or Azure App Service settings.</li>
        </ul>
    </li>
    <li><strong>Database Setup</strong>:
        <pre><code>dotnet ef database update</code></pre>
    </li>
    <li><strong>Run Services</strong> - Start individual services or the whole solution using Visual Studio or command line.</li>
</ol>

<h2 id="project-structure">Project Structure</h2>
<ul>
    <li><strong>Vin.Services.AuthAPI</strong>: Handles authentication and authorization.</li>
    <li><strong>Vin.Services.ProductAPI</strong>: Product catalog with endpoints for product data.</li>
    <li><strong>Vin.Services.ShoppingCartAPI</strong>: Manages cart operations and coupon applications.</li>
    <li><strong>Vin.Services.OrderAPI</strong>: Processes and manages orders.</li>
    <li><strong>Vin.Services.EmailAPI</strong>: Sends transactional emails.</li>
    <li><strong>Vin.Web</strong>: Client-side project for user interactions.</li>
</ul>

<h2 id="api-documentation">API Documentation</h2>
<p>API endpoints are documented in Swagger. Each microservice has its own Swagger UI for testing and exploring endpoints.</p>

<h2 id="deployment">Deployment</h2>
<p>Deployment is set up on <strong>Azure</strong>. Steps include:</p>
<ol>
    <li>Create Azure App Services for each microservice. (Comming soon)</li>
    <li>Set up Azure SQL Database and configure connection strings. (Comming soon)</li>
    <li>Enable Continuous Deployment from GitHub or other version control. (Comming soon)</li>
    <li>Configure Azure Service Bus and RabbitMQ for inter-service communication. (WIP)</li>
</ol>

<h2 id="future-enhancements">Future Enhancements</h2>
<ul>
    <li>Implementing <strong>Rewards API</strong> for loyalty points and rewards management.</li>
    <li>Adding an <strong>Admin Dashboard</strong> for managing products, orders, and users.</li>
    <li>Expanding <strong>Order Management</strong> with advanced tracking and reporting.</li>
    <li>Enhancing <strong>Checkout Experience</strong> with additional payment options.</li>
    <li>Integrating additional <strong>Message Queueing</strong> systems for redundancy.</li>
</ul>

</body>
</html>
