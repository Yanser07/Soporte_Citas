<script setup>
import { computed, watch, watchEffect } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { Bell, CalendarClock, ClipboardList, LogOut, Wrench } from 'lucide-vue-next'
import { useAuthStore } from '@/stores/auth'
import { useResumenStore } from '@/stores/resumen'
import { useAutoRefresh } from '@/composables/useAutoRefresh'

const auth = useAuthStore()
const resumen = useResumenStore()
const router = useRouter()

const iniciales = computed(() =>
  (auth.usuario?.nombre ?? '')
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((p) => p[0].toUpperCase())
    .join(''),
)

// Contador de pendientes: carga al entrar y se refresca cada 30 segundos
watch(
  () => auth.esAdmin,
  (esAdmin) => {
    if (esAdmin) resumen.cargar()
  },
  { immediate: true },
)
useAutoRefresh(() => {
  if (auth.esAdmin) resumen.cargar()
}, 30000)

// Título de la pestaña del navegador: "(3) Soporte Técnico"
watchEffect(() => {
  const n = auth.esAdmin ? resumen.conteos.pendientes : 0
  document.title = (n > 0 ? `(${n}) ` : '') + 'Soporte Técnico'
})

function salir() {
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <header class="sticky top-0 z-40 border-b border-slate-200/70 bg-white/80 backdrop-blur-md">
    <div class="mx-auto flex max-w-6xl items-center justify-between px-4 py-3 sm:px-6">
      <RouterLink to="/" class="flex items-center gap-3">
        <div
          class="grid size-10 place-items-center rounded-xl bg-linear-to-br from-blue-600 to-indigo-600 text-white shadow-md shadow-blue-600/25"
        >
          <Wrench class="size-5" />
        </div>
        <span class="text-lg font-bold tracking-tight text-slate-900">Soporte Técnico</span>
      </RouterLink>

      <nav v-if="auth.esAdmin" class="flex items-center gap-1">
        <RouterLink
          to="/admin"
          class="flex items-center gap-2 rounded-xl px-3 py-2 text-sm font-semibold text-slate-600 transition hover:bg-slate-100"
          active-class="bg-blue-50 !text-blue-700"
          exact-active-class="bg-blue-50 !text-blue-700"
        >
          <ClipboardList class="size-4" />
          <span class="hidden sm:inline">Solicitudes</span>
        </RouterLink>
        <RouterLink
          to="/admin/horario"
          class="flex items-center gap-2 rounded-xl px-3 py-2 text-sm font-semibold text-slate-600 transition hover:bg-slate-100"
          active-class="bg-blue-50 !text-blue-700"
        >
          <CalendarClock class="size-4" />
          <span class="hidden sm:inline">Horario</span>
        </RouterLink>
      </nav>

      <div v-if="auth.estaAutenticado" class="flex items-center gap-3">
        <RouterLink
          v-if="auth.esAdmin"
          to="/admin"
          class="relative grid size-10 place-items-center rounded-xl text-slate-600 transition hover:bg-slate-100"
          title="Solicitudes pendientes"
        >
          <Bell class="size-5" />
          <span
            v-if="resumen.conteos.pendientes > 0"
            class="absolute -right-0.5 -top-0.5 grid min-w-5 place-items-center rounded-full bg-rose-500 px-1 text-[11px] font-bold leading-5 text-white ring-2 ring-white"
          >
            {{ resumen.conteos.pendientes }}
          </span>
        </RouterLink>

        <RouterLink
          to="/perfil"
          class="flex items-center gap-3 rounded-full bg-slate-100/80 py-1 pl-1 pr-4 transition hover:bg-slate-200/70"
          title="Mi perfil"
        >
          <div
            class="grid size-8 place-items-center rounded-full bg-linear-to-br from-blue-500 to-indigo-600 text-xs font-bold text-white"
          >
            {{ iniciales }}
          </div>
          <div class="hidden leading-tight sm:block">
            <p class="text-sm font-semibold text-slate-800">{{ auth.usuario.nombre }}</p>
            <p class="text-xs text-slate-500">{{ auth.esAdmin ? 'Administrador' : 'Cliente' }}</p>
          </div>
        </RouterLink>

        <button
          class="grid size-10 place-items-center rounded-xl text-slate-500 transition hover:bg-rose-50 hover:text-rose-600"
          title="Cerrar sesión"
          @click="salir"
        >
          <LogOut class="size-5" />
        </button>
      </div>
    </div>
  </header>
</template>
