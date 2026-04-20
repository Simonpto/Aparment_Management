# PROJECT: Limnos Apartments Booking Platform

## Stack
- Frontend: Blazor (WebAssembly or Server)
- Backend: ASP.NET Core Web API (C#)
- Database: Supabase (PostgreSQL)
- Storage: Supabase Storage (images)
- Optional Auth: Supabase Auth (admin only or hybrid JWT)

---

## System Overview

A vacation rental platform for apartments in Limnos, Greece with:
- Public website (apartment listings + blog)
- Apartment detail pages with availability and pricing
- Guest review system via secure links
- Admin dashboard for managing content

---

# FRONTEND (Blazor)

## Pages

### 1. Home Page
- Hero section (Limnos branding)
- Featured apartments
- Quick navigation

---

### 2. Apartments Page
- Grid layout of all apartments
- Each card shows:
  - Image
  - Title
  - Price range
  - Short description

---

### 3. Apartment Detail Page
Includes:
- Image gallery
- Full description
- Amenities list
- Map (optional)
- Availability calendar
- Pricing display

---

### 4. Blog / Limnos Info Page
- List of blog posts
- Categories:
  - Beaches
  - Food
  - Travel guides
- Individual post view page

---

### 5. Reviews Section
- Display reviews per apartment
- Star rating system
- Review text

---

### 6. Review Submission Page (Token-Based)
- Accessed via secure link:
  `/review/{token}`
- Token is validated via backend
- Allows:
  - Rating
  - Review text submission

---

# BACKEND (ASP.NET Core Web API)

## Responsibilities
- Business logic
- Database communication (Supabase)
- Authentication validation
- Token generation for reviews
- Admin operations

---

## API Endpoints

### Apartments
- GET /api/apartments
- GET /api/apartments/{id}
- POST /api/apartments (admin)
- PUT /api/apartments/{id} (admin)
- DELETE /api/apartments/{id}

---

### Availability & Pricing
- GET /api/apartments/{id}/availability
- PUT /api/apartments/{id}/pricing
- PUT /api/apartments/{id}/availability

---

### Blog
- GET /api/blog
- GET /api/blog/{id}
- POST /api/blog (admin)
- PUT /api/blog/{id} (admin)

---

### Reviews
- GET /api/apartments/{id}/reviews
- POST /api/reviews (via token)
- POST /api/reviews/token/generate (admin)

---

### Review Tokens
- POST /api/review-tokens/create
- GET /api/review-tokens/validate/{token}

---

# DATABASE (SUPABASE)

## Tables

### apartments
- id (uuid)
- title
- description
- location
- amenities (json)
- created_at

---

### apartment_images
- id
- apartment_id
- image_url

---

### availability
- id
- apartment_id
- date
- is_available
- price_override

---

### reviews
- id
- apartment_id
- rating (1-5)
- text
- created_at
- approved

---

### review_tokens
- id
- apartment_id
- token (uuid)
- expires_at
- used (bool)

---

### blog_posts
- id
- title
- content
- image_url
- published_at

---

# STORAGE (SUPABASE)
- /apartments/{id}/images
- /blog/{postId}/images

---

# ADMIN FEATURES

Admin panel (Blazor or protected routes):

## Apartments
- Create/edit/delete apartments
- Upload images
- Set amenities

## Pricing
- Weekly/daily pricing
- Seasonal adjustments

## Availability
- Block/unblock dates
- Calendar view

## Blog
- Create/edit posts
- Publish/unpublish

## Reviews
- Approve/delete reviews
- Monitor submissions

## Review Links
- Generate secure token links:
  `/review/{token}`

---

# SECURITY

- Review tokens must be:
  - UUID-based
  - time-limited OR single-use
- Admin endpoints must be authenticated
- Input validation for all APIs
- Rate limiting on review submission

---

# UX REQUIREMENTS

- Mobile-first design
- Fast image loading
- Clean Airbnb-style layout
- Simple navigation
- High trust design (important for bookings)

---

# BUILD PHASES

### Phase 1
- Blazor UI skeleton
- Apartment listing
- Apartment detail page
- API basic integration

### Phase 2
- Supabase integration
- Image storage
- Blog system

### Phase 3
- Admin dashboard
- Pricing + availability system

### Phase 4
- Review system with tokens

### Phase 5
- Polish, SEO, performance, deployment