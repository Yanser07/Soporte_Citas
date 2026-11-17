import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'inicio',
      redirect: () => {
        const auth = useAuthStore()
        if (!auth.estaAutenticado) return '/login'
        return auth.esAdmin ? '/admin' : '/citas'
      },
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { soloInvitado: true },
    },
    {
      path: '/registro',
      name: 'registro',
      component: () => import('@/views/RegistroView.vue'),
      meta: { soloInvitado: true },
    },
    {
      path: '/olvide-password',
      name: 'olvide-password',
      component: () => import('@/views/OlvidePasswordView.vue'),
      meta: { soloInvitado: true },
    },
    {
      path: '/restablecer',
      name: 'restablecer',
      component: () => import('@/views/RestablecerPasswordView.vue'),
      meta: { soloInvitado: true },
    },
    {
      path: '/citas',
      name: 'citas',
      component: () => import('@/views/UsuarioCitasView.vue'),
      meta: { requiereAuth: true, rol: 'Usuario' },
    },
    {
      path: '/admin',
      name: 'admin',
      component: () => import('@/views/AdminCitasView.vue'),
      meta: { requiereAuth: true, rol: 'Admin' },
    },
    {
      path: '/admin/horario',
      name: 'admin-horario',
      component: () => import('@/views/AdminHorarioView.vue'),
      meta: { requiereAuth: true, rol: 'Admin' },
    },
    {
      path: '/perfil',
      name: 'perfil',
      component: () => import('@/views/PerfilView.vue'),
      meta: { requiereAuth: true },
    },
    { path: '/:pathMatch(.*)*', redirect: '/' },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  const destinoPropio = auth.esAdmin ? { name: 'admin' } : { name: 'citas' }

  if (to.meta.requiereAuth && !auth.estaAutenticado) return { name: 'login' }
  if (to.meta.soloInvitado && auth.estaAutenticado) return destinoPropio
  if (to.meta.rol && auth.usuario?.rol !== to.meta.rol) return destinoPropio
})

export default router