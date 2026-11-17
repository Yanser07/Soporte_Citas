import api from '@/services/api'

export async function obtenerHorario() {
  const { data } = await api.get('/admin/horario')
  return data
}

export async function guardarHorario(payload) {
  const { data } = await api.put('/admin/horario', payload)
  return data
}

export async function listarBloqueos() {
  const { data } = await api.get('/admin/horario/bloqueos')
  return data
}

export async function crearBloqueo(payload) {
  const { data } = await api.post('/admin/horario/bloqueos', payload)
  return data
}

export async function eliminarBloqueo(id) {
  await api.delete(`/admin/horario/bloqueos/${id}`)
}