<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import { KeyRound, Mail, Phone, Save, User } from 'lucide-vue-next'
import { actualizarPerfil, cambiarPassword, obtenerPerfil } from '@/services/perfilService'
import { useAuthStore } from '@/stores/auth'
import { formatearFecha } from '@/utils/formato'
import { mensajeDeError } from '@/utils/errores'

const auth = useAuthStore()

const perfil = ref(null)
const cargando = ref(false)
const errorCarga = ref('')

// --- Datos personales ---
const datos = reactive({ nombre: '', telefono: '' })
const guardandoDatos = ref(false)
const errorDatos = ref('')
const exitoDatos = ref('')

// --- Contraseña ---
const clave = reactive({ actual: '', nueva: '', confirmar: '' })
const guardandoClave = ref(false)
const errorClave = ref('')
const exitoClave = ref('')

const iniciales = computed(() =>
  (datos.nombre || '')
    .split(' ')
    .filter(Boolean)
    .slice(0, 2)
    .map((p) => p[0].toUpperCase())
    .join(''),
)

async function cargar() {
  cargando.value = true
  errorCarga.value = ''
  try {
    perfil.value = await obtenerPerfil()
    datos.nombre = perfil.value.nombre
    datos.telefono = perfil.value.telefono
  } catch (e) {
    errorCarga.value = mensajeDeError(e)
  } finally {
    cargando.value = false
  }
}

async function guardarDatos() {
  errorDatos.value = ''
  exitoDatos.value = ''
  guardandoDatos.value = true
  try {
    perfil.value = await actualizarPerfil({ nombre: datos.nombre, telefono: datos.telefono })
    auth.actualizarNombre(perfil.value.nombre) // se refleja en la barra superior
    exitoDatos.value = 'Datos actualizados.'
  } catch (e) {
    errorDatos.value = mensajeDeError(e)
  } finally {
    guardandoDatos.value = false
  }
}

async function guardarClave() {
  errorClave.value = ''
  exitoClave.value = ''

  if (clave.nueva !== clave.confirmar) {
    errorClave.value = 'Las contraseñas nuevas no coinciden.'
    return
  }

  guardandoClave.value = true
  try {
    await cambiarPassword(clave.actual, clave.nueva)
    exitoClave.value = 'Contraseña actualizada. Te enviamos un correo de confirmación.'
    clave.actual = ''
    clave.nueva = ''
    clave.confirmar = ''
  } catch (e) {
    errorClave.value = mensajeDeError(e)
  } finally {
    guardandoClave.value = false
  }
}

onMounted(cargar)
</script>

<template>
  <div>
    <div class="mb-8 flex items-center gap-4">
      <div
        class="grid size-16 place-items-center rounded-2xl bg-linear-to-br from-blue-500 to-indigo-600 text-xl font-bold text-white shadow-md shadow-blue-600/25"
      >
        {{ iniciales }}
      </div>
      <div>
        <h1 class="text-3xl font-bold tracking-tight text-slate-900">Mi perfil</h1>
        <p v-if="perfil" class="mt-0.5 text-sm text-slate-500">
          {{ perfil.rol === 'Admin' ? 'Administrador' : 'Cliente' }} · miembro desde
          {{ formatearFecha(perfil.fechaCreacion) }}
        </p>
      </div>
    </div>

    <p v-if="errorCarga" class="mb-6 rounded-xl bg-rose-50 p-3 text-sm text-rose-700 ring-1 ring-rose-200">
      {{ errorCarga }}
    </p>

    <div class="grid items-start gap-6 lg:grid-cols-2">
      <!-- Datos personales -->
      <section class="card p-6 sm:p-8">
        <h2 class="mb-6 text-lg font-bold text-slate-900">Datos personales</h2>

        <form class="space-y-5" @submit.prevent="guardarDatos">
          <div>
            <label class="label">Nombre completo</label>
            <div class="relative">
              <User class="absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
              <input v-model="datos.nombre" type="text" required maxlength="100" class="input pl-10" />
            </div>
          </div>

          <div>
            <label class="label">Teléfono</label>
            <div class="relative">
              <Phone class="absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
              <input v-model="datos.telefono" type="tel" maxlength="30" class="input pl-10" />
            </div>
          </div>

          <div>
            <label class="label">Correo electrónico</label>
            <div class="relative">
              <Mail class="absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
              <input
                :value="perfil?.correo ?? ''"
                type="email"
                disabled
                class="input cursor-not-allowed bg-slate-50 pl-10 text-slate-500"
              />
            </div>
            <p class="mt-1.5 text-xs text-slate-500">El correo no se puede modificar.</p>
          </div>

          <p v-if="errorDatos" class="rounded-xl bg-rose-50 p-3 text-sm text-rose-700 ring-1 ring-rose-200">
            {{ errorDatos }}
          </p>
          <p v-if="exitoDatos" class="rounded-xl bg-emerald-50 p-3 text-sm text-emerald-700 ring-1 ring-emerald-200">
            {{ exitoDatos }}
          </p>

          <button type="submit" :disabled="guardandoDatos || cargando" class="btn btn-primary w-full">
            <Save class="size-4" />
            {{ guardandoDatos ? 'Guardando...' : 'Guardar cambios' }}
          </button>
        </form>
      </section>

      <!-- Cambiar contraseña -->
      <section class="card p-6 sm:p-8">
        <h2 class="mb-6 text-lg font-bold text-slate-900">Cambiar contraseña</h2>

        <form class="space-y-5" @submit.prevent="guardarClave">
          <div>
            <label class="label">Contraseña actual</label>
            <div class="relative">
              <KeyRound class="absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
              <input
                v-model="clave.actual"
                type="password"
                required
                autocomplete="current-password"
                class="input pl-10"
              />
            </div>
          </div>

          <div>
            <label class="label">Nueva contraseña</label>
            <div class="relative">
              <KeyRound class="absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
              <input
                v-model="clave.nueva"
                type="password"
                required
                minlength="8"
                maxlength="64"
                pattern="(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,64}"
                title="Entre 8 y 64 caracteres, con al menos una mayúscula, una minúscula y un número."
                autocomplete="new-password"
                class="input pl-10"
              />
            </div>
            <p class="mt-1.5 text-xs text-slate-500">
              Mínimo 8 caracteres, con mayúscula, minúscula y número.
            </p>
          </div>

          <div>
            <label class="label">Confirmar nueva contraseña</label>
            <div class="relative">
              <KeyRound class="absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
              <input
                v-model="clave.confirmar"
                type="password"
                required
                autocomplete="new-password"
                class="input pl-10"
              />
            </div>
          </div>

          <p v-if="errorClave" class="rounded-xl bg-rose-50 p-3 text-sm text-rose-700 ring-1 ring-rose-200">
            {{ errorClave }}
          </p>
          <p v-if="exitoClave" class="rounded-xl bg-emerald-50 p-3 text-sm text-emerald-700 ring-1 ring-emerald-200">
            {{ exitoClave }}
          </p>

          <button type="submit" :disabled="guardandoClave" class="btn btn-primary w-full">
            {{ guardandoClave ? 'Actualizando...' : 'Actualizar contraseña' }}
          </button>
        </form>
      </section>
    </div>
  </div>
</template>