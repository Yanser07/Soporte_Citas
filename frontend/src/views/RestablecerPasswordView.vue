<script setup>
import { reactive, ref } from 'vue'
import { RouterLink, useRoute } from 'vue-router'
import { CheckCircle2, Lock, TriangleAlert } from 'lucide-vue-next'
import AuthLayout from '@/components/AuthLayout.vue'
import { restablecerPassword } from '@/services/passwordService'
import { mensajeDeError } from '@/utils/errores'

const route = useRoute()
const token = typeof route.query.token === 'string' ? route.query.token : ''

const form = reactive({ password: '', confirmar: '' })
const cargando = ref(false)
const listo = ref(false)
const error = ref('')

async function enviar() {
  error.value = ''

  if (form.password !== form.confirmar) {
    error.value = 'Las contraseñas no coinciden.'
    return
  }

  cargando.value = true
  try {
    await restablecerPassword(token, form.password)
    listo.value = true
  } catch (e) {
    error.value = mensajeDeError(e)
  } finally {
    cargando.value = false
  }
}
</script>

<template>
  <AuthLayout titulo="Crea una nueva contraseña" subtitulo="Elige una contraseña segura que no uses en otros sitios.">
    <!-- Enlace sin token -->
    <div v-if="!token" class="rounded-2xl bg-amber-50 p-6 text-center ring-1 ring-amber-200">
      <div class="mx-auto mb-3 grid size-12 place-items-center rounded-2xl bg-amber-100 text-amber-600">
        <TriangleAlert class="size-6" />
      </div>
      <p class="font-semibold text-amber-800">Enlace incompleto</p>
      <p class="mt-1 text-sm text-amber-700">
        Abre el enlace completo que te enviamos por correo, o solicita uno nuevo.
      </p>
      <RouterLink to="/olvide-password" class="btn btn-primary mt-4">Solicitar nuevo enlace</RouterLink>
    </div>

    <!-- Contraseña cambiada -->
    <div v-else-if="listo" class="rounded-2xl bg-emerald-50 p-6 text-center ring-1 ring-emerald-200">
      <div class="mx-auto mb-3 grid size-12 place-items-center rounded-2xl bg-emerald-100 text-emerald-600">
        <CheckCircle2 class="size-6" />
      </div>
      <p class="font-semibold text-emerald-800">¡Contraseña actualizada!</p>
      <p class="mt-1 text-sm text-emerald-700">Ya puedes iniciar sesión con tu nueva contraseña.</p>
      <RouterLink to="/login" class="btn btn-primary mt-4">Ir a iniciar sesión</RouterLink>
    </div>

    <!-- Formulario -->
    <form v-else class="space-y-5" @submit.prevent="enviar">
      <div>
        <label class="label">Nueva contraseña</label>
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
        {{ cargando ? 'Guardando...' : 'Guardar contraseña' }}
      </button>
    </form>
  </AuthLayout>
</template>