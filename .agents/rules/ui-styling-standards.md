---
trigger: glob
globs: *.ts, *.html, *.scss
---

# UI and Styling Rules (Angular)
* **Framework:** Use Tailwind CSS for all styling. Do not write custom CSS/SCSS unless absolutely necessary for complex animations.
* **Component Structure:** Use Angular Standalone Components exclusively. 
* **Design Language:** The dashboard should have a clean, modern aesthetic (e.g., white cards, subtle shadows, rounded corners `rounded-lg`).
* **Responsiveness:** Ensure all views (Dashboard, Settings, Login) use Tailwind utility classes (`md:`, `lg:`) to be fully responsive.