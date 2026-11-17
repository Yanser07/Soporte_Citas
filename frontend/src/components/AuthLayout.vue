<script setup>
import { CalendarCheck, Mail, ShieldCheck, Wrench } from 'lucide-vue-next'

defineProps({
  titulo: { type: String, required: true },
  subtitulo: { type: String, default: '' },
})
</script>

<template>
  <div class="grid min-h-screen lg:grid-cols-2">
    <aside
      class="relative hidden overflow-hidden bg-linear-to-br from-blue-600 via-blue-700 to-indigo-800 p-12 text-white lg:flex lg:flex-col lg:justify-between"
    >
      <div class="absolute -right-24 -top-24 size-80 rounded-full bg-white/10 blur-2xl"></div>
      <div class="absolute -bottom-32 -left-20 size-96 rounded-full bg-indigo-400/20 blur-3xl"></div>

      <div class="relative flex items-center gap-3">
        <div class="grid size-11 place-items-center rounded-2xl bg-white/15 ring-1 ring-white/25">
          <Wrench class="size-6" />
        </div>
        <span class="text-xl font-bold">Soporte Técnico</span>
      </div>

      <div class="relative space-y-8">
        <h2 class="text-4xl font-bold leading-tight">
          Agenda tu soporte técnico sin complicaciones
        </h2>
        <ul class="space-y-4 text-blue-50">
          <li class="flex items-start gap-3">
            <CalendarCheck class="mt-0.5 size-5 shrink-0" />
            <span>Elige el día y la hora que mejor te convenga.</span>
          </li>
          <li class="flex items-start gap-3">
            <Mail class="mt-0.5 size-5 shrink-0" />
            <span>Recibe la respuesta directamente en tu correo.</span>
          </li>
          <li class="flex items-start gap-3">
            <ShieldCheck class="mt-0.5 size-5 shrink-0" />
            <span>Tus datos se guardan de forma segura.</span>
          </li>
        </ul>
      </div>

      <p class="relative text-sm text-blue-100/80">© {{ new Date().getFullYear() }} Soporte Técnico</p>
    </aside>

    <section class="flex items-center justify-center p-6 sm:p-12">
      <div class="animate-fade-up w-full max-w-md">
        <div class="mb-8 flex items-center gap-3 lg:hidden">
          <div
            class="grid size-10 place-items-center rounded-xl bg-linear-to-br from-blue-600 to-indigo-600 text-white"
          >
            <Wrench class="size-5" />
          </div>
          <span class="text-lg font-bold">Soporte Técnico</span>
        </div>

        <h1 class="text-3xl font-bold tracking-tight text-slate-900">{{ titulo }}</h1>
        <p v-if="subtitulo" class="mt-2 text-slate-500">{{ subtitulo }}</p>

        <div class="mt-8"><slot /></div>

        <div class="mt-6 text-center text-sm text-slate-600"><slot name="pie" /></div>
      </div>
    </section>
  </div>
</template>