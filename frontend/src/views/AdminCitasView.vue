<script setup>
import { computed, onMounted, onUnmounted, ref, watch } from 'vue'
import { Inbox, RefreshCw, Search, X } from 'lucide-vue-next'
import CitaAdminCard from '@/components/CitaAdminCard.vue'
import CompletarCitaModal from '@/components/CompletarCitaModal.vue'
import Paginacion from '@/components/Paginacion.vue'
import ResponderCitaModal from '@/components/ResponderCitaModal.vue'
import { completarCita, listarCitas, responderCita } from '@/services/adminCitasService'
import { useResumenStore } from '@/stores/resumen'
import { useAutoRefresh } from '@/composables/useAutoRefresh'
import { mensajeDeError } from '@/utils/errores'

const TAMANO_PAGINA = 10

const resumen = useResumenStore()

const filtros = [
  { etiqueta: 'Pendientes', valor: 'Pendiente', clave: 'pendientes' },
  { etiqueta: 'Aceptadas', valor: 'Aceptada', clave: 'aceptadas' },
  { etiqueta: 'Completadas', valor: 'Completada', clave: 'completadas' },
  { etiqueta: 'Rechazadas', valor: 'Rechazada', clave: 'rechazadas' },
  { etiqueta: 'Canceladas', valor: 'Cancelada', clave: 'canceladas' },
  { etiqueta: 'Todas', valor: '', clave: 'total' },
]

// --- Filtros y paginación ---
const filtro = ref('Pendiente')
const buscar = ref('')
const desde = ref('')
const hasta = ref('')
const pagina = ref(1)
const totalCitas = ref(0)
const totalPaginas = ref(1)

const hayFiltros = computed(() => !!(buscar.value || desde.value || hasta.value))

// --- Estado de la pantalla ---
const citas = ref([])
const cargando = ref(false)
const error = ref('')
const exito = ref('')
const ultimaActualizacion = ref(null)

// Aceptar / rechazar: { cita, aceptar }
const seleccion = ref(null)
const enviando = ref(false)
const errorModal = ref('')

// Completar: la cita elegida
const citaCompletar = ref(null)
const completando = ref(false)
const errorCompletar = ref('')

// Cada petición lleva un número; solo la última puede actualizar la pantalla
// (evita que una respuesta lenta pise a otra más reciente)
let numeroSolicitud = 0

// silencioso = true: actualización automática, sin indicador de carga ni mensajes de error
async function cargar(silencioso = false) {
  const solicitud = ++numeroSolicitud

  if (!silencioso) {
    cargando.value = true
    error.value = ''
  }
  try {
    const resultado = await listarCitas({
      estado: filtro.value,
      buscar: buscar.value.trim(),
      desde: desde.value,
      hasta: hasta.value,
      pagina: pagina.value,
      tamanoPagina: TAMANO_PAGINA,
    })

    if (solicitud === numeroSolicitud) {
      citas.value = resultado.items
      totalCitas.value = resultado.total
      totalPaginas.value = resultado.totalPaginas
      pagina.value = resultado.pagina // el servidor corrige la página si ya no existe
      ultimaActualizacion.value = new Date()
    }
  } catch (e) {
    if (!silencioso && solicitud === numeroSolicitud) error.value = mensajeDeError(e)
  } finally {
    if (!silencioso && solicitud === numeroSolicitud) cargando.value = false
  }
}

function cambiarPagina(nueva) {
  pagina.value = nueva
  cargar()
}

function limpiarFiltros() {
  buscar.value = ''
  desde.value = ''
  hasta.value = ''
}

// La búsqueda espera un momento después de dejar de escribir
let temporizadorBusqueda = null
watch(buscar, () => {
  clearTimeout(temporizadorBusqueda)
  temporizadorBusqueda = setTimeout(() => {
    pagina.value = 1
    cargar()
  }, 350)
})

watch([filtro, desde, hasta], () => {
  exito.value = ''
  pagina.value = 1
  cargar()
})

// --- Aceptar / rechazar ---
function abrirModal(cita, aceptar) {
  errorModal.value = ''
  exito.value = ''
  seleccion.value = { cita, aceptar }
}

function cerrarModal() {
  seleccion.value = null
}

async function confirmar(motivo) {
  enviando.value = true
  errorModal.value = ''
  try {
    await responderCita(seleccion.value.cita.id, seleccion.value.aceptar, motivo)
    cerrarModal()
    exito.value = 'Respuesta registrada. El cliente recibirá un correo con el resultado.'
    await Promise.all([cargar(), resumen.cargar()])
  } catch (e) {
    errorModal.value = mensajeDeError(e)
    await Promise.all([cargar(true), resumen.cargar()]) // por si la cita cambió de estado
  } finally {
    enviando.value = false
  }
}

// --- Completar ---
function abrirCompletar(cita) {
  errorCompletar.value = ''
  exito.value = ''
  citaCompletar.value = cita
}

function cerrarCompletar() {
  citaCompletar.value = null
}

async function confirmarCompletar(datos) {
  completando.value = true
  errorCompletar.value = ''
  try {
    await completarCita(citaCompletar.value.id, datos)
    cerrarCompletar()
    exito.value = 'Cita marcada como completada. El cliente recibirá un correo con el resumen.'
    await Promise.all([cargar(), resumen.cargar()])
  } catch (e) {
    errorCompletar.value = mensajeDeError(e)
    await Promise.all([cargar(true), resumen.cargar()])
  } finally {
    completando.value = false
  }
}

onMounted(() => cargar())
onUnmounted(() => clearTimeout(temporizadorBusqueda))
useAutoRefresh(() => cargar(true), 30000)
</script>

<template>
  <div>
    <div class="mb-8 flex items-end justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold tracking-tight text-slate-900">Solicitudes de soporte</h1>
        <p class="mt-1 flex items-center gap-2 text-sm text-slate-500">
          <span class="relative flex size-2">
            <span class="absolute inline-flex size-full animate-ping rounded-full bg-emerald-400 opacity-75"></span>
            <span class="relative inline-flex size-2 rounded-full bg-emerald-500"></span>
          </span>
          Se actualiza automáticamente
          <span v-if="ultimaActualizacion">· última vez {{ ultimaActualizacion.toLocaleTimeString('es-DO') }}</span>
        </p>
      </div>
      <button class="btn btn-secondary" @click="() => { cargar(); resumen.cargar() }">
        <RefreshCw class="size-4" :class="{ 'animate-spin': cargando }" />
        <span class="hidden sm:inline">Actualizar</span>
      </button>
    </div>

    <!-- Pestañas por estado -->
    <div class="mb-4 flex flex-wrap gap-2">
      <button
        v-for="f in filtros"
        :key="f.etiqueta"
        class="flex items-center gap-2 rounded-full px-4 py-2 text-sm font-semibold transition"
        :class="
          filtro === f.valor
            ? 'bg-blue-600 text-white shadow-md shadow-blue-600/25'
            : 'bg-white text-slate-600 ring-1 ring-slate-200 hover:bg-slate-50'
        "
        @click="filtro = f.valor"
      >
        {{ f.etiqueta }}
        <span
          class="grid min-w-5 place-items-center rounded-full px-1.5 text-xs font-bold leading-5"
          :class="
            filtro === f.valor
              ? 'bg-white/25 text-white'
              : f.clave === 'pendientes' && resumen.conteos.pendientes > 0
                ? 'bg-amber-100 text-amber-700'
                : 'bg-slate-100 text-slate-500'
          "
        >
          {{ resumen.conteos[f.clave] }}
        </span>
      </button>
    </div>

    <!-- Búsqueda y fechas -->
    <div class="card mb-6 grid gap-3 p-4 sm:grid-cols-[1fr_auto_auto_auto] sm:items-end">
      <div>
        <label class="label">Buscar</label>
        <div class="relative">
          <Search class="absolute left-3.5 top-1/2 size-4 -translate-y-1/2 text-slate-400" />
          <input
            v-model="buscar"
            type="search"
            maxlength="100"
            placeholder="Cliente, correo, teléfono o servicio..."
            class="input pl-10"
          />
        </div>
      </div>

      <div>
        <label class="label">Desde</label>
        <input v-model="desde" type="date" :max="hasta || undefined" class="input" />
      </div>

      <div>
        <label class="label">Hasta</label>
        <input v-model="hasta" type="date" :min="desde || undefined" class="input" />
      </div>

      <button v-if="hayFiltros" class="btn btn-secondary" @click="limpiarFiltros">
        <X class="size-4" /> Limpiar
      </button>
    </div>

    <p v-if="error" class="mb-4 rounded-xl bg-rose-50 p-3 text-sm text-rose-700 ring-1 ring-rose-200">
      {{ error }}
    </p>
    <p v-if="exito" class="mb-4 rounded-xl bg-emerald-50 p-3 text-sm text-emerald-700 ring-1 ring-emerald-200">
      {{ exito }}
    </p>

    <div v-if="cargando && !citas.length" class="grid gap-4 md:grid-cols-2">
      <div v-for="n in 4" :key="n" class="h-56 animate-pulse rounded-2xl bg-white/70 ring-1 ring-slate-900/5"></div>
    </div>

    <div v-else-if="!citas.length" class="card py-16 text-center">
      <div class="mx-auto mb-3 grid size-14 place-items-center rounded-2xl bg-slate-100 text-slate-400">
        <Inbox class="size-7" />
      </div>
      <p class="font-medium text-slate-700">
        {{ hayFiltros ? 'No hay resultados para tu búsqueda' : 'No hay solicitudes en esta categoría' }}
      </p>
      <p class="text-sm text-slate-500">
        {{ hayFiltros ? 'Prueba con otros términos o limpia los filtros.' : 'Cuando lleguen nuevas, aparecerán aquí automáticamente.' }}
      </p>
    </div>

    <template v-else>
      <ul class="grid gap-4 md:grid-cols-2">
        <CitaAdminCard
          v-for="cita in citas"
          :key="cita.id"
          :cita="cita"
          @aceptar="abrirModal($event, true)"
          @rechazar="abrirModal($event, false)"
          @completar="abrirCompletar"
        />
      </ul>

      <Paginacion
        :pagina="pagina"
        :total-paginas="totalPaginas"
        :total="totalCitas"
        :tamano-pagina="TAMANO_PAGINA"
        @cambiar="cambiarPagina"
      />
    </template>

    <Transition name="fade">
      <ResponderCitaModal
        v-if="seleccion"
        :cita="seleccion.cita"
        :aceptar="seleccion.aceptar"
        :enviando="enviando"
        :error="errorModal"
        @confirmar="confirmar"
        @cerrar="cerrarModal"
      />
    </Transition>

    <Transition name="fade">
      <CompletarCitaModal
        v-if="citaCompletar"
        :cita="citaCompletar"
        :enviando="completando"
        :error="errorCompletar"
        @confirmar="confirmarCompletar"
        @cerrar="cerrarCompletar"
      />
    </Transition>
  </div>
</template>