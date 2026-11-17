<script setup>
import { computed, ref } from 'vue'
import { formatearFechaHora } from '@/utils/formato'

const props = defineProps({
  cita: { type: Object, required: true },
  aceptar: { type: Boolean, required: true },
  enviando: { type: Boolean, default: false },
  error: { type: String, default: '' },
})

const emit = defineEmits(['confirmar', 'cerrar'])

const motivo = ref('')

const titulo = computed(() => (props.aceptar ? 'Aceptar solicitud' : 'Rechazar solicitud'))
const etiqueta = computed(() =>
  props.aceptar ? 'Comentario para el cliente (opcional)' : 'Motivo del rechazo (obligatorio)',
)

function enviar() {
  emit('confirmar', motivo.value.trim())
}
</script>

<template>
  <div
    class="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 p-4 backdrop-blur-sm"
    @click.self="emit('cerrar')"
  >
    <form
      class="animate-fade-up w-full max-w-md rounded-2xl bg-white p-6 shadow-2xl"
      @submit.prevent="enviar"
    >
      <h2 class="text-xl font-bold text-slate-900">{{ titulo }}</h2>
      <p class="mt-1 text-sm text-slate-500">
        {{ cita.clienteNombre }} · {{ cita.tipoServicio }}<br />
        <span class="capitalize">{{ formatearFechaHora(cita.fechaHora) }}</span>
      </p>

      <label class="label mt-5">{{ etiqueta }}</label>
      <textarea
        v-model="motivo"
        :required="!aceptar"
        rows="4"
        maxlength="500"
        class="input resize-none"
      ></textarea>
      <p class="mt-1.5 text-xs text-slate-500">Este texto se enviará al cliente por correo.</p>

      <p v-if="error" class="mt-3 rounded-xl bg-rose-50 p-3 text-sm text-rose-700 ring-1 ring-rose-200">
        {{ error }}
      </p>

      <div class="mt-6 flex justify-end gap-2">
        <button type="button" class="btn btn-secondary" @click="emit('cerrar')">Cancelar</button>
        <button type="submit" :disabled="enviando" class="btn" :class="aceptar ? 'btn-success' : 'btn-danger'">
          {{ enviando ? 'Enviando...' : aceptar ? 'Confirmar y aceptar' : 'Confirmar y rechazar' }}
        </button>
      </div>
    </form>
  </div>
</template>