<script setup>
import { computed } from 'vue'
import { RouterView, useRoute } from 'vue-router'
import NavBar from '@/components/NavBar.vue'

const route = useRoute()
const esPaginaAuth = computed(() => !!route.meta.soloInvitado)
</script>

<template>
  <div class="min-h-screen bg-linear-to-br from-slate-50 via-blue-50/50 to-indigo-50/50">
    <NavBar v-if="!esPaginaAuth" />
    <main :class="esPaginaAuth ? '' : 'mx-auto max-w-6xl px-4 py-8 sm:px-6'">
      <RouterView />
    </main>
  </div>
</template>