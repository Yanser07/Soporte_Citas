<script setup>
import { ref } from 'vue'
import { formatearFechaHora } from '@/utils/formato'

defineProps({
  cita: { type: Object, required: true },
  enviando: { type: Boolean, default: false },
  error: { type: String, default: '' },
})

const emit = defineEmits(['confirmar', 'cerrar'])

const resumenTrabajo = ref('')
const notasInternas = ref('')

function enviar() {
  emit('confirmar', {
    resumenTrabajo: resumenTrabajo.value.trim(),
    notasInternas: notasInternas.value.trim(),
  })
}
</script>

<template>
  <div
    class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 p-4 backdrop-blur-sm"
    @click.self="emit('cerrar')"
  >
    <form
      class="animate-fade-up max-h-[90vh] w-full max-w-lg overflow-y-auto rounded-2xl bg-white p-6 shadow-2xl"
      @submit.prevent="enviar"
    >
      <h2 class="text-xl font-bold text-slate-900">Marcar como completada</h2>
      <p class="mt-1 text-sm text-slate-500">
        {{ cita.clienteNombre }} · {{ cita.tipoServicio }}<br />
        <span class="capitalize">{{ formatearFechaHora(cita.fechaHora) }}</span>
      </p>

      <label class="label mt-5">Resumen del trabajo realizado (opcional)</label>
      <textarea
        v-model="resumenTrabajo"
        rows="4"
        maxlength="1000"
        placeholder="Ej.: Se reemplazó la fuente de poder y se actualizó el sistema."
        class="input resize-none"
      ></textarea>
      <p class="mt-1.5 text-xs text-slate-500">El cliente lo verá en su historial y le llegará por correo.</p>

      <label class="label mt-5">Notas internas (opcional)</label>
      <textarea
        v-model="notasInternas"
        rows="3"
        maxlength="1000"
        placeholder="Solo las ves tú: detalles técnicos, cobros pendientes..."
        class="input resize-none"
      ></textarea>
      <p class="mt-1.5 text-xs text-slate-500">Estas notas son privadas y nunca se envían al cliente.</p>

      <p v-if="error" class="mt-3 rounded-xl bg-rose-50 p-3 text-sm text-rose-700 ring-1 ring-rose-200">
        {{ error }}
      </p>

      <div class="mt-6 flex justify-end gap-2">
        <button type="button" class="btn btn-secondary" @click="emit('cerrar')">Cancelar</button>
        <button type="submit" :disabled="enviando" class="btn btn-primary">
          {{ enviando ? 'Guardando...' : 'Completar cita' }}
        </button>
      </div>
    </form>
  </div>
</template>