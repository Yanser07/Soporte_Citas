<script setup>
import { computed } from 'vue'

const props = defineProps({
  estado: { type: String, required: true },
})

const estilos = {
  Pendiente: { etiqueta: 'bg-amber-50 text-amber-700 ring-amber-600/20', punto: 'bg-amber-500' },
  Aceptada: { etiqueta: 'bg-emerald-50 text-emerald-700 ring-emerald-600/20', punto: 'bg-emerald-500' },
  Completada: { etiqueta: 'bg-indigo-50 text-indigo-700 ring-indigo-600/20', punto: 'bg-indigo-500' },
  Rechazada: { etiqueta: 'bg-rose-50 text-rose-700 ring-rose-600/20', punto: 'bg-rose-500' },
  Cancelada: { etiqueta: 'bg-slate-100 text-slate-600 ring-slate-500/20', punto: 'bg-slate-400' },
}

const estilo = computed(() => estilos[props.estado] ?? estilos.Cancelada)
</script>

<template>
  <span
    class="inline-flex items-center gap-1.5 rounded-full px-2.5 py-1 text-xs font-semibold ring-1 ring-inset"
    :class="estilo.etiqueta"
  >
    <span class="size-1.5 rounded-full" :class="estilo.punto"></span>
    {{ estado }}
  </span>
</template>