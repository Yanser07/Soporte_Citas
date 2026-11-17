<script setup>
import { reactive, ref } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { ArrowRight, Lock, Mail } from 'lucide-vue-next'
import AuthLayout from '@/components/AuthLayout.vue'
import { useAuthStore } from '@/stores/auth'
import { mensajeDeError } from '@/utils/errores'

const auth = useAuthStore()
const router = useRouter()

const form = reactive({ correo: '', password: '' })
const error = ref('')
const cargando = ref(false)

async function enviar() {
  error.value = ''
  cargando.value = true
  try {
    const sesion = await auth.login(form.correo, form.password)
    router.push(sesion.rol === 'Admin' ? '/admin' : '/citas')
  } catch (e) {
    error.value = mensajeDeError(e)
  } finally {
    cargando.value = false
  }
}
</script>

<template>
  <AuthLayout titulo="Bienvenido de nuevo" subtitulo="Inicia sesión para gestionar tus citas.">
    <form class="space-y-5" @submit.prevent="enviar">
      <div>
        <label class="label">Correo electrónico</label>
        <div class="relative">
          <Mail class="absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
          <input
            v-model="form.correo"
            type="email"
            required
            placeholder="tucorreo@ejemplo.com"
            class="input pl-10"
          />
        </div>
      </div>

            <div>
        <div class="mb-1.5 flex items-center justify-between">
          <label class="text-sm font-medium text-slate-700">Contraseña</label>
          <RouterLink to="/olvide-password" class="text-sm font-semibold text-blue-600 hover:underline">
            ¿Olvidaste tu contraseña?
          </RouterLink>
        </div>
        <div class="relative">
          <Lock class="absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
          <input
            v-model="form.password"
            type="password"
            required
            placeholder="••••••••"
            class="input pl-10"
          />
        </div>
      </div>

      <p v-if="error" class="rounded-xl bg-rose-50 p-3 text-sm text-rose-700 ring-1 ring-rose-200">
        {{ error }}
      </p>

      <button type="submit" :disabled="cargando" class="btn btn-primary w-full">
        {{ cargando ? 'Entrando...' : 'Entrar' }}
        <ArrowRight v-if="!cargando" class="size-4" />
      </button>
    </form>

    <template #pie>
      ¿No tienes cuenta?
      <RouterLink to="/registro" class="font-semibold text-blue-600 hover:underline">
        Regístrate
      </RouterLink>
    </template>
  </AuthLayout>
</template>