<script setup>
import { ref } from 'vue'
import { RouterLink } from 'vue-router'
import { ArrowLeft, MailCheck, Mail } from 'lucide-vue-next'
import AuthLayout from '@/components/AuthLayout.vue'
import { solicitarRecuperacion } from '@/services/passwordService'
import { mensajeDeError } from '@/utils/errores'

const correo = ref('')
const cargando = ref(false)
const enviado = ref(false)
const error = ref('')

async function enviar() {
  error.value = ''
  cargando.value = true
  try {
    await solicitarRecuperacion(correo.value)
    enviado.value = true
  } catch (e) {
    error.value = mensajeDeError(e)
  } finally {
    cargando.value = false
  }
}
</script>

<template>
  <AuthLayout
    titulo="¿Olvidaste tu contraseña?"
    subtitulo="Escribe tu correo y te enviaremos un enlace para crear una nueva."
  >
    <div v-if="enviado" class="rounded-2xl bg-emerald-50 p-6 text-center ring-1 ring-emerald-200">
      <div class="mx-auto mb-3 grid size-12 place-items-center rounded-2xl bg-emerald-100 text-emerald-600">
        <MailCheck class="size-6" />
      </div>
      <p class="font-semibold text-emerald-800">Revisa tu correo</p>
      <p class="mt-1 text-sm text-emerald-700">
        Si <b>{{ correo }}</b> está registrado, recibirás un enlace en unos instantes. Revisa también la
        carpeta de spam. El enlace dura 30 minutos.
      </p>
    </div>

    <form v-else class="space-y-5" @submit.prevent="enviar">
      <div>
        <label class="label">Correo electrónico</label>
        <div class="relative">
          <Mail class="absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
          <input
            v-model="correo"
            type="email"
            required
            placeholder="tucorreo@ejemplo.com"
            class="input pl-10"
          />
        </div>
      </div>

      <p v-if="error" class="rounded-xl bg-rose-50 p-3 text-sm text-rose-700 ring-1 ring-rose-200">
        {{ error }}
      </p>

      <button type="submit" :disabled="cargando" class="btn btn-primary w-full">
        {{ cargando ? 'Enviando...' : 'Enviar enlace' }}
      </button>
    </form>

    <template #pie>
      <RouterLink to="/login" class="inline-flex items-center gap-1.5 font-semibold text-blue-600 hover:underline">
        <ArrowLeft class="size-4" /> Volver a iniciar sesión
      </RouterLink>
    </template>
  </AuthLayout>
</template>