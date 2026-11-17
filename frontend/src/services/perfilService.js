import api from '@/services/api'

export async function obtenerPerfil() {
  const { data } = await api.get('/perfil')
  return data
}

export async function actualizarPerfil({ nombre, telefono }) {
  const { data } = await api.put('/perfil', { nombre, telefono })
  return data
}

export async function cambiarPassword(passwordActual, nuevaPassword) {
  const { data } = await api.post('/perfil/cambiar-password', { passwordActual, nuevaPassword })
  return data
}