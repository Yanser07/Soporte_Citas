<script setup>
import { reactive, ref } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { ArrowRight, Lock, Mail, Phone, User } from 'lucide-vue-next'
import AuthLayout from '@/components/AuthLayout.vue'
import { useAuthStore } from '@/stores/auth'
import { mensajeDeError } from '@/utils/errores'

const auth = useAuthStore()
const router = useRouter()

const form = reactive({ nombre: '', correo: '', telefono: '', password: '', confirmar: '' })
const error = ref('')
const cargando = ref(false)

async function enviar() {
  error.value = ''

  if (form.password !== form.confirmar) {
    error.value = 'Las contraseñas no coinciden.'
    return
  }

  cargando.value = true
  try {
    await auth.registro({
      nombre: form.nombre,
      correo: form.correo,
      telefono: form.telefono,
      password: form.password,
    })
    router.push('/citas')
  } catch (e) {
    error.value = mensajeDeError(e)
  } finally {
    cargando.value = false
  }
}
</script>

<template>
  <AuthLayout titulo="Crea tu cuenta" subtitulo="Solo te tomará un minuto.">
    <form class="space-y-4" @submit.prevent="enviar">
      <div>
        <label class="label">Nombre completo</label>
        <div class="relative">
          <User class="absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
          <input v-model="form.nombre" type="text" required maxlength="100" class="input pl-10" />
        </div>
      </div>

      <div>
        <label class="label">Correo electrónico</label>
        <div class="relative">
          <Mail class="absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
          <input v-model="form.correo" type="email" required class="input pl-10" />
        </div>
      </div>

      <div>
        <label class="label">Teléfono</label>
        <div class="relative">
          <Phone class="absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
          <input v-model="form.telefono" type="tel" class="input pl-10" />
        </div>
      </div>

      <div>
        <label class="label">Contraseña</label>
        <div class="relative">
          <Lock class="absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
          <input
            v-model="form.password"
            type="password"
            required
            minlength="8"
            maxlength="64"
            pattern="(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,64}"
            title="Entre 8 y 64 caracteres, con al menos una mayúscula, una minúscula y un número."
            class="input pl-10"
          />
        </div>
        <p class="mt-1.5 text-xs text-slate-500">
          Mínimo 8 caracteres, con mayúscula, minúscula y número.
        </p>
      </div>

      <div>
        <label class="label">Confirmar contraseña</label>
        <div class="relative">
          <Lock class="absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
          <input v-model="form.confirmar" type="password" required class="input pl-10" />
        </div>
      </div>

      <p v-if="error" class="rounded-xl bg-rose-50 p-3 text-sm text-rose-700 ring-1 ring-rose-200">
        {{ error }}
      </p>

      <button type="submit" :disabled="cargando" class="btn btn-primary w-full">
        {{ cargando ? 'Creando cuenta...' : 'Crear cuenta' }}
        <ArrowRight v-if="!cargando" class="size-4" />
      </button>
    </form>

    <template #pie>
      ¿Ya tienes cuenta?
      <RouterLink to="/login" class="font-semibold text-blue-600 hover:underline">
        Inicia sesión
      </RouterLink>
    </template>
  </AuthLayout>
</template>