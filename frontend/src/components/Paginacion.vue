<script setup>
import { computed } from 'vue'
import { ChevronLeft, ChevronRight } from 'lucide-vue-next'

const props = defineProps({
  pagina: { type: Number, required: true },
  totalPaginas: { type: Number, required: true },
  total: { type: Number, required: true },
  tamanoPagina: { type: Number, required: true },
})

const emit = defineEmits(['cambiar'])

const desde = computed(() => (props.total === 0 ? 0 : (props.pagina - 1) * props.tamanoPagina + 1))
const hasta = computed(() => Math.min(props.pagina * props.tamanoPagina, props.total))

// Ventana de hasta 5 números de página alrededor de la actual
const numeros = computed(() => {
  const inicio = Math.max(1, Math.min(props.pagina - 2, props.totalPaginas - 4))
  const fin = Math.min(props.totalPaginas, inicio + 4)
  const lista = []
  for (let i = inicio; i <= fin; i++) lista.push(i)
  return lista
})
</script>

<template>
  <div class="mt-6 flex flex-col items-center justify-between gap-3 sm:flex-row">
    <p class="text-sm text-slate-500">
      Mostrando <span class="font-semibold text-slate-700">{{ desde }}–{{ hasta }}</span> de
      <span class="font-semibold text-slate-700">{{ total }}</span>
    </p>

    <nav v-if="totalPaginas > 1" class="flex items-center gap-1">
      <button
        class="grid size-9 place-items-center rounded-xl bg-white text-slate-600 ring-1 ring-slate-200 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40"
        :disabled="pagina <= 1"
        title="Anterior"
        @click="emit('cambiar', pagina - 1)"
      >
        <ChevronLeft class="size-4" />
      </button>

      <button
        v-for="n in numeros"
        :key="n"
        class="grid size-9 place-items-center rounded-xl text-sm font-semibold transition"
        :class="
          n === pagina
            ? 'bg-blue-600 text-white shadow-md shadow-blue-600/25'
            : 'bg-white text-slate-600 ring-1 ring-slate-200 hover:bg-slate-50'
        "
        @click="emit('cambiar', n)"
      >
        {{ n }}
      </button>

      <button
        class="grid size-9 place-items-center rounded-xl bg-white text-slate-600 ring-1 ring-slate-200 transition hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-40"
        :disabled="pagina >= totalPaginas"
        title="Siguiente"
        @click="emit('cambiar', pagina + 1)"
      >
        <ChevronRight class="size-4" />
      </button>
    </nav>
  </div>
</template>