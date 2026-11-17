<script setup>
import { CalendarX, CircleCheck, Clock } from 'lucide-vue-next'
import EstadoBadge from '@/components/EstadoBadge.vue'
import { partesFecha } from '@/utils/formato'
import { infoModalidad } from '@/utils/modalidades'

defineProps({
  citas: { type: Array, required: true },
  cargando: { type: Boolean, default: false },
})

const emit = defineEmits(['cancelar'])

function puedeCancelar(cita) {
  return cita.estado === 'Pendiente' || cita.estado === 'Aceptada'
}

function pedirCancelacion(cita) {
  if (window.confirm('¿Seguro que quieres cancelar esta cita?')) {
    emit('cancelar', cita.id)
  }
}
</script>

<template>
  <section class="card p-6 sm:p-8">
    <h2 class="mb-6 text-lg font-bold text-slate-900">Mis citas</h2>

    <div v-if="cargando && !citas.length" class="space-y-3">
      <div v-for="n in 3" :key="n" class="h-24 animate-pulse rounded-2xl bg-slate-100"></div>
    </div>

    <div v-else-if="!citas.length" class="py-10 text-center">
      <div class="mx-auto mb-3 grid size-14 place-items-center rounded-2xl bg-slate-100 text-slate-400">
        <CalendarX class="size-7" />
      </div>
      <p class="font-medium text-slate-700">Aún no tienes citas</p>
      <p class="text-sm text-slate-500">Cuando agendes una, aparecerá aquí.</p>
    </div>

    <ul v-else class="space-y-3">
      <li
        v-for="cita in citas"
        :key="cita.id"
        class="animate-fade-up flex gap-4 rounded-2xl border border-slate-100 bg-slate-50/60 p-4 transition hover:border-slate-200 hover:bg-white hover:shadow-sm"
      >
        <div
          class="flex size-16 shrink-0 flex-col items-center justify-center rounded-xl bg-white text-center shadow-sm ring-1 ring-slate-900/5"
        >
          <span class="text-xl font-bold leading-none text-blue-600">{{ partesFecha(cita.fechaHora).dia }}</span>
          <span class="mt-1 text-[11px] font-semibold uppercase text-slate-500">{{ partesFecha(cita.fechaHora).mes }}</span>
        </div>

        <div class="min-w-0 flex-1">
          <div class="flex items-start justify-between gap-2">
            <p class="font-semibold text-slate-900">{{ cita.tipoServicio }}</p>
            <EstadoBadge :estado="cita.estado" />
          </div>

          <p class="mt-0.5 flex items-center gap-1.5 text-sm capitalize text-slate-500">
            <Clock class="size-3.5" />
            {{ partesFecha(cita.fechaHora).diaSemana }}, {{ partesFecha(cita.fechaHora).hora }}
          </p>

          <p class="mt-1 flex items-start gap-1.5 text-sm text-slate-500">
            <component :is="infoModalidad(cita.modalidad).icono" class="mt-0.5 size-3.5 shrink-0" />
            <span>
              {{ infoModalidad(cita.modalidad).etiqueta }}
              <span v-if="cita.modalidad === 'Domicilio' && cita.direccion"> · {{ cita.direccion }}</span>
            </span>
          </p>

          <p class="mt-2 text-sm text-slate-600">{{ cita.descripcion }}</p>

          <p
            v-if="cita.motivoRespuesta"
            class="mt-3 rounded-xl bg-white p-3 text-sm text-slate-700 ring-1 ring-slate-200"
          >
            <span class="font-semibold">
              {{ cita.estado === 'Rechazada' ? 'Motivo del rechazo:' : 'Comentario del administrador:' }}
            </span>
            {{ cita.motivoRespuesta }}
          </p>

          <p
            v-if="cita.estado === 'Completada'"
            class="mt-3 flex items-start gap-2 rounded-xl bg-indigo-50 p-3 text-sm text-indigo-900 ring-1 ring-indigo-200"
          >
            <CircleCheck class="mt-0.5 size-4 shrink-0 text-indigo-600" />
            <span>
              <span class="font-semibold">Servicio completado.</span>
              <span v-if="cita.resumenTrabajo" class="whitespace-pre-line"> {{ cita.resumenTrabajo }}</span>
            </span>
          </p>

          <button
            v-if="puedeCancelar(cita)"
            class="mt-3 text-sm font-semibold text-rose-600 hover:underline"
            @click="pedirCancelacion(cita)"
          >
            Cancelar cita
          </button>
        </div>
      </li>
    </ul>
  </section>
</template>