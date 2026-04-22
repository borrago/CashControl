import { createRouter, createWebHistory, type RouteRecordRaw } from "vue-router";
import { useSessionStore } from "@/modules/auth/application/session.store";
import HomeView from "@/modules/home/presentation/HomeView.vue";
import LoginView from "@/modules/auth/presentation/LoginView.vue";
import RegisterView from "@/modules/auth/presentation/RegisterView.vue";
import ForgotPasswordView from "@/modules/auth/presentation/ForgotPasswordView.vue";
import ResetPasswordView from "@/modules/auth/presentation/ResetPasswordView.vue";
import ConfirmEmailView from "@/modules/auth/presentation/ConfirmEmailView.vue";
import ProfileView from "@/modules/users/presentation/ProfileView.vue";
import AdminUsersView from "@/modules/admin/presentation/AdminUsersView.vue";

const routes: RouteRecordRaw[] = [
  { path: "/", name: "home", component: HomeView },
  { path: "/login", name: "login", component: LoginView, meta: { guestOnly: true } },
  { path: "/register", name: "register", component: RegisterView, meta: { guestOnly: true } },
  { path: "/forgot-password", name: "forgot-password", component: ForgotPasswordView, meta: { guestOnly: true } },
  { path: "/reset-password", name: "reset-password", component: ResetPasswordView, meta: { guestOnly: true } },
  { path: "/confirm-email", name: "confirm-email", component: ConfirmEmailView, meta: { guestOnly: true } },
  { path: "/profile", name: "profile", component: ProfileView, meta: { requiresAuth: true } },
  { path: "/admin/users", name: "admin-users", component: AdminUsersView, meta: { requiresAuth: true, requiresAdmin: true } },
];

export const router = createRouter({
  history: createWebHistory(),
  routes,
});

router.beforeEach(async (to) => {
  const sessionStore = useSessionStore();

  if (!sessionStore.isBootstrapped) {
    await sessionStore.bootstrap();
  }

  if (to.meta.requiresAuth && !sessionStore.isAuthenticated) {
    return {
      name: "login",
      query: {
        redirect: to.fullPath,
      },
    };
  }

  if (to.meta.guestOnly && sessionStore.isAuthenticated) {
    return { name: "profile" };
  }

  if (to.meta.requiresAdmin && !sessionStore.isAdmin) {
    return { name: "profile" };
  }

  return true;
});
