import api from '@/services/api'

export async function solicitarRecuperacion(correo) {
  const { data } = await api.post('/password/olvide', { correo })
  return data
}

export async function restablecerPassword(token, nuevaPassword) {
  const { data } = await api.post('/password/restablecer', { token, nuevaPassword })
  return data
}