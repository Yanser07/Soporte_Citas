<script setup>
import { ref, watch } from 'vue'
import { CalendarPlus } from 'lucide-vue-next'
import { crearCita, obtenerDisponibilidad } from '@/services/citasService'
import { formatearHora, hoyLocal } from '@/utils/formato'
import { modalidades } from '@/utils/modalidades'
import { mensajeDeError } from '@/utils/errores'

const emit = defineEmits(['creada'])

const tiposServicio = [
  'Reparación de computadora',
  'Instalación de software',
  'Redes e internet',
  'Recuperación de datos',
  'Eliminación de virus',
  'Configuración de impresoras',
  'Otro',
]

const hoy = hoyLocal()
const fecha = ref('')
const horarios = ref([])
const mensajeDia = ref('')
const horaElegida = ref('')
const tipoServicio = ref('')
const modalidad = ref('')
const direccion = ref('')
const descripcion = ref('')

const cargandoHorarios = ref(false)
const enviando = ref(false)
const error = ref('')
const exito = ref('')

async function cargarHorarios() {
  horaElegida.value = ''
  horarios.value = []
  mensajeDia.value = ''
  if (!fecha.value) return

  cargandoHorarios.value = true
  try {
    const disponibilidad = await obtenerDisponibilidad(fecha.value)
    horarios.value = disponibilidad.horarios
    mensajeDia.value = disponibilidad.mensaje ?? ''
  } catch (e) {
    error.value = mensajeDeError(e)
  } finally {
    cargandoHorarios.value = false
  }
}

watch(fecha, () => {
  error.value = ''
  exito.value = ''
  cargarHorarios()
})

async function enviar() {
  error.value = ''
  exito.value = ''

  if (!horaElegida.value) {
    error.value = 'Selecciona un horario disponible.'
    return
  }
  if (!modalidad.value) {
    error.value = 'Selecciona la modalidad de atención.'
    return
  }

  enviando.value = true
  try {
    await crearCita({
      fechaHora: horaElegida.value,
      tipoServicio: tipoServicio.value,
      modalidad: modalidad.value,
      direccion: modalidad.value === 'Domicilio' ? direccion.value : null,
      descripcion: descripcion.value,
    })
    exito.value = 'Solicitud enviada. Te avisaremos por correo cuando el administrador responda.'
    tipoServicio.value = ''
    modalidad.value = ''
    direccion.value = ''
    descripcion.value = ''
    emit('creada')
  } catch (e) {
    error.value = mensajeDeError(e)
  } finally {
    enviando.value = false
    await cargarHorarios() // refresca los horarios (por si alguien más ocupó uno)
  }
}

// Permite que la vista padre refresque los horarios (por ejemplo, tras cancelar una cita)
defineExpose({ cargarHorarios })
</script>

<template>
  <section class="card p-6 sm:p-8">
    <div class="mb-6 flex items-center gap-3">
      <div class="grid size-10 place-items-center rounded-xl bg-blue-50 text-blue-600">
        <CalendarPlus class="size-5" />
      </div>
      <div>
        <h2 class="text-lg font-bold text-slate-900">Agendar una cita</h2>
        <p class="text-sm text-slate-500">Elige fecha, horario y cuéntanos el problema.</p>
      </div>
    </div>

    <form class="space-y-6" @submit.prevent="enviar">
      <!-- 1. Fecha -->
      <div>
        <label class="label">
          <span
            class="mr-2 inline-grid size-5 place-items-center rounded-full bg-blue-600 text-[11px] font-bold text-white"
            >1</span
          >
          Fecha
        </label>
        <input v-model="fecha" type="date" :min="hoy" required class="input" />
      </div>

      <!-- 2. Horario -->
      <div v-if="fecha">
        <label class="label">
          <span
            class="mr-2 inline-grid size-5 place-items-center rounded-full bg-blue-600 text-[11px] font-bold text-white"
            >2</span
          >
          Horario
        </label>

        <div v-if="cargandoHorarios" class="grid grid-cols-3 gap-2 sm:grid-cols-4">
          <div v-for="n in 8" :key="n" class="h-10 animate-pulse rounded-xl bg-slate-100"></div>
        </div>

        <p
          v-else-if="mensajeDia"
          class="rounded-xl bg-amber-50 p-3 text-sm text-amber-800 ring-1 ring-amber-200"
        >
          {{ mensajeDia }} Prueba con otra fecha.
        </p>

        <template v-else>
          <div class="grid grid-cols-3 gap-2 sm:grid-cols-4">
            <button
              v-for="h in horarios"
              :key="h.fechaHora"
              type="button"
              :disabled="!h.disponible"
              :title="h.motivoNoDisponible ?? ''"
              class="rounded-xl border px-2 py-2.5 text-sm font-medium transition"
              :class="
                horaElegida === h.fechaHora
                  ? 'border-blue-600 bg-blue-600 text-white shadow-md shadow-blue-600/25'
                  : h.disponible
                    ? 'border-slate-200 bg-white text-slate-700 hover:border-blue-400 hover:bg-blue-50'
                    : 'cursor-not-allowed border-transparent bg-slate-100 text-slate-400 line-through'
              "
              @click="horaElegida = h.fechaHora"
            >
              {{ formatearHora(h.fechaHora) }}
            </button>
          </div>
          <p
            v-if="horarios.length && !horarios.some((h) => h.disponible)"
            class="mt-2 text-sm text-slate-500"
          >
            No hay horarios disponibles ese día. Prueba con otra fecha.
          </p>
        </template>
      </div>

      <!-- 3. Tipo de servicio -->
      <div>
        <label class="label">
          <span
            class="mr-2 inline-grid size-5 place-items-center rounded-full bg-blue-600 text-[11px] font-bold text-white"
            >3</span
          >
          Tipo de servicio
        </label>
        <select v-model="tipoServicio" required class="input">
          <option value="" disabled>Selecciona una opción</option>
          <option v-for="t in tiposServicio" :key="t" :value="t">{{ t }}</option>
        </select>
      </div>

      <!-- 4. Modalidad -->
      <div>
        <label class="label">
          <span
            class="mr-2 inline-grid size-5 place-items-center rounded-full bg-blue-600 text-[11px] font-bold text-white"
            >4</span
          >
          ¿Cómo prefieres la atención?
        </label>
        <div class="grid gap-2 sm:grid-cols-3">
          <button
            v-for="m in modalidades"
            :key="m.valor"
            type="button"
            class="rounded-xl border p-3 text-left transition"
            :class="
              modalidad === m.valor
                ? 'border-blue-600 bg-blue-50 ring-2 ring-blue-600/20'
                : 'border-slate-200 bg-white hover:border-blue-400'
            "
            @click="modalidad = m.valor"
          >
            <component
              :is="m.icono"
              class="mb-2 size-5"
              :class="modalidad === m.valor ? 'text-blue-600' : 'text-slate-400'"
            />
            <p class="text-sm font-semibold text-slate-800">{{ m.etiqueta }}</p>
            <p class="mt-0.5 text-xs text-slate-500">{{ m.descripcion }}</p>
          </button>
        </div>

        <div v-if="modalidad === 'Domicilio'" class="mt-3">
          <label class="label">Dirección del servicio</label>
          <input
            v-model="direccion"
            type="text"
            required
            maxlength="300"
            placeholder="Calle, número, sector, referencia..."
            class="input"
          />
        </div>
      </div>

      <!-- 5. Descripción -->
      <div>
        <label class="label">
          <span
            class="mr-2 inline-grid size-5 place-items-center rounded-full bg-blue-600 text-[11px] font-bold text-white"
            >5</span
          >
          Describe el problema
        </label>
        <textarea
          v-model="descripcion"
          required
          rows="4"
          maxlength="1000"
          placeholder="Cuéntanos qué está pasando..."
          class="input resize-none"
        ></textarea>
      </div>

      <p v-if="error" class="rounded-xl bg-rose-50 p-3 text-sm text-rose-700 ring-1 ring-rose-200">
        {{ error }}
      </p>
      <p
        v-if="exito"
        class="rounded-xl bg-emerald-50 p-3 text-sm text-emerald-700 ring-1 ring-emerald-200"
      >
        {{ exito }}
      </p>

      <button type="submit" :disabled="enviando" class="btn btn-primary w-full">
        {{ enviando ? 'Enviando...' : 'Solicitar cita' }}
      </button>
    </form>
  </section>
</template>
