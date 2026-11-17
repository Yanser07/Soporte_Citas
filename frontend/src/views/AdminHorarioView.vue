<script setup>
import { onMounted, reactive, ref } from 'vue'
import { CalendarOff, Save, Trash2 } from 'lucide-vue-next'
import {
  crearBloqueo,
  eliminarBloqueo,
  guardarHorario,
  listarBloqueos,
  obtenerHorario,
} from '@/services/horarioAdminService'
import { etiquetaHora, formatearFecha, hoyLocal } from '@/utils/formato'
import { mensajeDeError } from '@/utils/errores'

const diasSemana = [
  { valor: 1, etiqueta: 'Lunes' },
  { valor: 2, etiqueta: 'Martes' },
  { valor: 3, etiqueta: 'Miércoles' },
  { valor: 4, etiqueta: 'Jueves' },
  { valor: 5, etiqueta: 'Viernes' },
  { valor: 6, etiqueta: 'Sábado' },
  { valor: 0, etiqueta: 'Domingo' },
]
const horasInicio = Array.from({ length: 24 }, (_, i) => i) // 0 a 23
const horasFin = Array.from({ length: 24 }, (_, i) => i + 1) // 1 a 24

const hoy = hoyLocal()

// --- Horario general ---
const config = reactive({ diasLaborables: [], horaInicio: 8, horaFin: 18 })
const guardando = ref(false)
const errorConfig = ref('')
const exitoConfig = ref('')

// --- Bloqueos ---
const bloqueos = ref([])
const nuevo = reactive({
  fechaInicio: '',
  fechaFin: '',
  todoElDia: true,
  horaDesde: 13,
  horaHasta: 18,
  motivo: '',
})
const creando = ref(false)
const errorBloqueo = ref('')
const avisoBloqueo = ref('')

const cargando = ref(false)
const errorGeneral = ref('')

async function cargarTodo() {
  cargando.value = true
  errorGeneral.value = ''
  try {
    const [horario, lista] = await Promise.all([obtenerHorario(), listarBloqueos()])
    Object.assign(config, horario)
    bloqueos.value = lista
  } catch (e) {
    errorGeneral.value = mensajeDeError(e)
  } finally {
    cargando.value = false
  }
}

async function guardarConfig() {
  errorConfig.value = ''
  exitoConfig.value = ''

  if (!config.diasLaborables.length) {
    errorConfig.value = 'Selecciona al menos un día laborable.'
    return
  }

  guardando.value = true
  try {
    const guardado = await guardarHorario({
      diasLaborables: config.diasLaborables,
      horaInicio: Number(config.horaInicio),
      horaFin: Number(config.horaFin),
    })
    Object.assign(config, guardado)
    exitoConfig.value = 'Horario guardado.'
  } catch (e) {
    errorConfig.value = mensajeDeError(e)
  } finally {
    guardando.value = false
  }
}

async function agregarBloqueo() {
  errorBloqueo.value = ''
  avisoBloqueo.value = ''

  creando.value = true
  try {
    const resultado = await crearBloqueo({
      fechaInicio: nuevo.fechaInicio,
      fechaFin: nuevo.fechaFin || nuevo.fechaInicio,
      horaDesde: nuevo.todoElDia ? null : Number(nuevo.horaDesde),
      horaHasta: nuevo.todoElDia ? null : Number(nuevo.horaHasta),
      motivo: nuevo.motivo,
    })

    if (resultado.citasAfectadas > 0) {
      avisoBloqueo.value = `Bloqueo creado. Atención: hay ${resultado.citasAfectadas} cita(s) pendiente(s) o aceptada(s) en ese periodo. No se cancelaron: revísalas en Solicitudes.`
    }

    nuevo.fechaInicio = ''
    nuevo.fechaFin = ''
    nuevo.todoElDia = true
    nuevo.motivo = ''
    bloqueos.value = await listarBloqueos()
  } catch (e) {
    errorBloqueo.value = mensajeDeError(e)
  } finally {
    creando.value = false
  }
}

async function quitarBloqueo(bloqueo) {
  if (!window.confirm('¿Eliminar este bloqueo? Esos horarios volverán a estar disponibles.')) return
  try {
    await eliminarBloqueo(bloqueo.id)
    bloqueos.value = await listarBloqueos()
  } catch (e) {
    errorBloqueo.value = mensajeDeError(e)
  }
}

function describirFechas(b) {
  const inicio = formatearFecha(b.fechaInicio)
  const fin = formatearFecha(b.fechaFin)
  return inicio === fin ? inicio : `${inicio} – ${fin}`
}

function describirHoras(b) {
  return b.horaDesde === null || b.horaDesde === undefined
    ? 'Todo el día'
    : `${etiquetaHora(b.horaDesde)} a ${etiquetaHora(b.horaHasta)}`
}

onMounted(cargarTodo)
</script>

<template>
  <div>
    <div class="mb-8">
      <h1 class="text-3xl font-bold tracking-tight text-slate-900">Horario de atención</h1>
      <p class="mt-1 text-slate-500">
        Define qué días y horas atiendes, y bloquea feriados, vacaciones o ratos ocupados.
      </p>
    </div>

    <p v-if="errorGeneral" class="mb-6 rounded-xl bg-rose-50 p-3 text-sm text-rose-700 ring-1 ring-rose-200">
      {{ errorGeneral }}
    </p>

    <div class="grid items-start gap-6 lg:grid-cols-2">
      <!-- Horario general -->
      <section class="card p-6 sm:p-8">
        <h2 class="mb-1 text-lg font-bold text-slate-900">Horario general</h2>
        <p class="mb-6 text-sm text-slate-500">Se aplica a todos los días laborables.</p>

        <form class="space-y-6" @submit.prevent="guardarConfig">
          <div>
            <label class="label">Días en que atiendes</label>
            <div class="flex flex-wrap gap-2">
              <label
                v-for="d in diasSemana"
                :key="d.valor"
                class="cursor-pointer select-none rounded-xl border px-3 py-2 text-sm font-medium transition"
                :class="
                  config.diasLaborables.includes(d.valor)
                    ? 'border-blue-600 bg-blue-600 text-white shadow-md shadow-blue-600/25'
                    : 'border-slate-200 bg-white text-slate-600 hover:border-blue-400'
                "
              >
                <input v-model="config.diasLaborables" type="checkbox" :value="d.valor" class="sr-only" />
                {{ d.etiqueta }}
              </label>
            </div>
          </div>

          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="label">Abres a las</label>
              <select v-model="config.horaInicio" class="input">
                <option v-for="h in horasInicio" :key="h" :value="h">{{ etiquetaHora(h) }}</option>
              </select>
            </div>
            <div>
              <label class="label">Cierras a las</label>
              <select v-model="config.horaFin" class="input">
                <option v-for="h in horasFin" :key="h" :value="h">{{ etiquetaHora(h) }}</option>
              </select>
            </div>
          </div>

          <p class="text-xs text-slate-500">
            Las citas ya existentes no se modifican si cambias el horario.
          </p>

          <p v-if="errorConfig" class="rounded-xl bg-rose-50 p-3 text-sm text-rose-700 ring-1 ring-rose-200">
            {{ errorConfig }}
          </p>
          <p v-if="exitoConfig" class="rounded-xl bg-emerald-50 p-3 text-sm text-emerald-700 ring-1 ring-emerald-200">
            {{ exitoConfig }}
          </p>

          <button type="submit" :disabled="guardando || cargando" class="btn btn-primary w-full">
            <Save class="size-4" />
            {{ guardando ? 'Guardando...' : 'Guardar horario' }}
          </button>
        </form>
      </section>

      <!-- Bloqueos -->
      <section class="card p-6 sm:p-8">
        <h2 class="mb-1 text-lg font-bold text-slate-900">Fechas y horas bloqueadas</h2>
        <p class="mb-6 text-sm text-slate-500">Los clientes no podrán reservar en estos periodos.</p>

        <form class="space-y-4" @submit.prevent="agregarBloqueo">
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="label">Desde</label>
              <input v-model="nuevo.fechaInicio" type="date" :min="hoy" required class="input" />
            </div>
            <div>
              <label class="label">Hasta (opcional)</label>
              <input v-model="nuevo.fechaFin" type="date" :min="nuevo.fechaInicio || hoy" class="input" />
            </div>
          </div>

          <label class="flex cursor-pointer items-center gap-2 text-sm font-medium text-slate-700">
            <input v-model="nuevo.todoElDia" type="checkbox" class="size-4 rounded border-slate-300 text-blue-600" />
            Bloquear el día completo
          </label>

          <div v-if="!nuevo.todoElDia" class="grid grid-cols-2 gap-4">
            <div>
              <label class="label">De las</label>
              <select v-model="nuevo.horaDesde" class="input">
                <option v-for="h in horasInicio" :key="h" :value="h">{{ etiquetaHora(h) }}</option>
              </select>
            </div>
            <div>
              <label class="label">Hasta las</label>
              <select v-model="nuevo.horaHasta" class="input">
                <option v-for="h in horasFin" :key="h" :value="h">{{ etiquetaHora(h) }}</option>
              </select>
            </div>
          </div>

          <div>
            <label class="label">Motivo (los clientes lo verán)</label>
            <input
              v-model="nuevo.motivo"
              type="text"
              maxlength="200"
              placeholder="Ej.: Feriado, vacaciones..."
              class="input"
            />
          </div>

          <p v-if="errorBloqueo" class="rounded-xl bg-rose-50 p-3 text-sm text-rose-700 ring-1 ring-rose-200">
            {{ errorBloqueo }}
          </p>
          <p v-if="avisoBloqueo" class="rounded-xl bg-amber-50 p-3 text-sm text-amber-800 ring-1 ring-amber-200">
            {{ avisoBloqueo }}
          </p>

          <button type="submit" :disabled="creando" class="btn btn-primary w-full">
            <CalendarOff class="size-4" />
            {{ creando ? 'Bloqueando...' : 'Agregar bloqueo' }}
          </button>
        </form>

        <div class="mt-8">
          <h3 class="mb-3 text-sm font-semibold uppercase tracking-wide text-slate-500">Bloqueos vigentes</h3>

          <p v-if="!bloqueos.length" class="text-sm text-slate-500">No hay bloqueos programados.</p>

          <ul v-else class="space-y-2">
            <li
              v-for="b in bloqueos"
              :key="b.id"
              class="flex items-start justify-between gap-3 rounded-xl border border-slate-100 bg-slate-50/60 p-3"
            >
              <div class="min-w-0">
                <p class="font-semibold text-slate-800">{{ describirFechas(b) }}</p>
                <p class="text-sm text-slate-500">
                  {{ describirHoras(b) }}<span v-if="b.motivo"> · {{ b.motivo }}</span>
                </p>
              </div>
              <button
                class="grid size-9 shrink-0 place-items-center rounded-lg text-slate-400 transition hover:bg-rose-50 hover:text-rose-600"
                title="Eliminar bloqueo"
                @click="quitarBloqueo(b)"
              >
                <Trash2 class="size-4" />
              </button>
            </li>
          </ul>
        </div>
      </section>
    </div>
  </div>
</template>