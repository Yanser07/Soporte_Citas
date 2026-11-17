<script setup>
import { computed } from 'vue'
import { Check, CircleCheck, Clock, Lock, Mail, Phone, X } from 'lucide-vue-next'
import EstadoBadge from '@/components/EstadoBadge.vue'
import { partesFecha } from '@/utils/formato'
import { infoModalidad } from '@/utils/modalidades'

const props = defineProps({
  cita: { type: Object, required: true },
})

defineEmits(['aceptar', 'rechazar', 'completar'])

// Solo se puede completar una cita aceptada cuando ya llegó su hora
const yaLlegoLaHora = computed(() => new Date(props.cita.fechaHora) <= new Date())

const enlaceMapa = computed(
  () => `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(props.cita.direccion ?? '')}`,
)
</script>

<template>
  <li class="card animate-fade-up flex flex-col p-5 transition hover:shadow-md">
    <div class="flex gap-4">
      <div
        class="flex size-16 shrink-0 flex-col items-center justify-center rounded-xl bg-blue-50 text-center"
      >
        <span class="text-xl font-bold leading-none text-blue-600">{{ partesFecha(cita.fechaHora).dia }}</span>
        <span class="mt-1 text-[11px] font-semibold uppercase text-blue-500">{{ partesFecha(cita.fechaHora).mes }}</span>
      </div>

      <div class="min-w-0 flex-1">
        <div class="flex items-start justify-between gap-2">
          <p class="text-lg font-semibold text-slate-900">{{ cita.tipoServicio }}</p>
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
            <a
              v-if="cita.modalidad === 'Domicilio' && cita.direccion"
              :href="enlaceMapa"
              target="_blank"
              rel="noopener noreferrer"
              class="text-blue-600 hover:underline"
            >
              · {{ cita.direccion }}
            </a>
          </span>
        </p>
      </div>
    </div>

    <div class="mt-4 rounded-xl bg-slate-50 p-3 text-sm">
      <p class="font-semibold text-slate-800">{{ cita.clienteNombre }}</p>
      <a
        :href="`mailto:${cita.clienteCorreo}`"
        class="mt-1 flex items-center gap-2 text-slate-600 hover:text-blue-600"
      >
        <Mail class="size-3.5" /> {{ cita.clienteCorreo }}
      </a>
      <a
        v-if="cita.clienteTelefono"
        :href="`tel:${cita.clienteTelefono}`"
        class="mt-1 flex items-center gap-2 text-slate-600 hover:text-blue-600"
      >
        <Phone class="size-3.5" /> {{ cita.clienteTelefono }}
      </a>
    </div>

    <p class="mt-4 whitespace-pre-line text-sm text-slate-600">{{ cita.descripcion }}</p>

    <p
      v-if="cita.motivoRespuesta"
      class="mt-4 rounded-xl bg-slate-50 p-3 text-sm text-slate-700 ring-1 ring-slate-200"
    >
      <span class="font-semibold">Tu respuesta:</span> {{ cita.motivoRespuesta }}
    </p>

    <div
      v-if="cita.estado === 'Completada'"
      class="mt-4 space-y-2 rounded-xl bg-indigo-50 p-3 text-sm text-indigo-900 ring-1 ring-indigo-200"
    >
      <p class="flex items-center gap-2 font-semibold"><CircleCheck class="size-4" /> Trabajo completado</p>
      <p v-if="cita.resumenTrabajo" class="whitespace-pre-line">
        <span class="font-semibold">Resumen enviado al cliente:</span> {{ cita.resumenTrabajo }}
      </p>
      <p v-if="cita.notasInternas" class="whitespace-pre-line rounded-lg bg-white/70 p-2 text-slate-700">
        <span class="flex items-center gap-1.5 font-semibold"><Lock class="size-3.5" /> Notas internas (solo tú)</span>
        {{ cita.notasInternas }}
      </p>
    </div>

    <div v-if="cita.estado === 'Pendiente'" class="mt-5 flex gap-2">
      <button class="btn btn-success flex-1" @click="$emit('aceptar', cita)">
        <Check class="size-4" /> Aceptar
      </button>
      <button class="btn btn-danger flex-1" @click="$emit('rechazar', cita)">
        <X class="size-4" /> Rechazar
      </button>
    </div>

    <div v-else-if="cita.estado === 'Aceptada'" class="mt-5">
      <button v-if="yaLlegoLaHora" class="btn btn-primary w-full" @click="$emit('completar', cita)">
        <CircleCheck class="size-4" /> Marcar como completada
      </button>
      <p v-else class="text-center text-xs text-slate-500">
        Podrás marcarla como completada cuando llegue su hora.
      </p>
    </div>
  </li>
</template>