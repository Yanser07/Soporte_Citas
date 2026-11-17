<script setup>
import { computed, onMounted, ref } from 'vue'
import { RefreshCw } from 'lucide-vue-next'
import AgendarCitaForm from '@/components/AgendarCitaForm.vue'
import MisCitasLista from '@/components/MisCitasLista.vue'
import { cancelarCita, obtenerMisCitas } from '@/services/citasService'
import { useAuthStore } from '@/stores/auth'
import { useAutoRefresh } from '@/composables/useAutoRefresh'
import { mensajeDeError } from '@/utils/errores'

const auth = useAuthStore()
const primerNombre = computed(() => auth.usuario?.nombre?.split(' ')[0] ?? '')

const citas = ref([])
const cargando = ref(false)
const error = ref('')
const formulario = ref(null)

async function cargarCitas(silencioso = false) {
  if (!silencioso) {
    cargando.value = true
    error.value = ''
  }
  try {
    citas.value = await obtenerMisCitas()
  } catch (e) {
    if (!silencioso) error.value = mensajeDeError(e)
  } finally {
    if (!silencioso) cargando.value = false
  }
}

async function cancelar(id) {
  error.value = ''
  try {
    await cancelarCita(id)
    await cargarCitas()
    await formulario.value?.cargarHorarios() // el horario liberado vuelve a aparecer
  } catch (e) {
    error.value = mensajeDeError(e)
  }
}

onMounted(() => cargarCitas())
useAutoRefresh(() => cargarCitas(true), 60000)
</script>

<template>
  <div>
    <div class="mb-8 flex items-end justify-between gap-4">
      <div>
        <h1 class="text-3xl font-bold tracking-tight text-slate-900">Hola, {{ primerNombre }} 👋</h1>
        <p class="mt-1 text-slate-500">Agenda una cita de soporte y revisa el estado de tus solicitudes.</p>
      </div>
      <button class="btn btn-secondary" @click="cargarCitas()">
        <RefreshCw class="size-4" :class="{ 'animate-spin': cargando }" />
        <span class="hidden sm:inline">Actualizar</span>
      </button>
    </div>

    <p v-if="error" class="mb-6 rounded-xl bg-rose-50 p-3 text-sm text-rose-700 ring-1 ring-rose-200">
      {{ error }}
    </p>

    <div class="grid items-start gap-6 lg:grid-cols-2">
      <AgendarCitaForm ref="formulario" @creada="cargarCitas()" />
      <MisCitasLista :citas="citas" :cargando="cargando" @cancelar="cancelar" />
    </div>
  </div>
</template>