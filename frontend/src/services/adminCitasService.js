import api from '@/services/api'

// Devuelve { items, total, pagina, tamanoPagina, totalPaginas }
export async function listarCitas({ estado, buscar, desde, hasta, pagina, tamanoPagina }) {
  const { data } = await api.get('/admin/citas', {
    params: {
      estado: estado || undefined,
      buscar: buscar || undefined,
      desde: desde || undefined,
      hasta: hasta || undefined,
      pagina,
      tamanoPagina,
    },
  })
  return data
}

export async function obtenerResumen() {
  const { data } = await api.get('/admin/citas/resumen')
  return data
}

export async function responderCita(id, aceptar, motivo) {
  const { data } = await api.post(`/admin/citas/${id}/responder`, {
    aceptar,
    motivo: motivo || null,
  })
  return data
}

export async function completarCita(id, { resumenTrabajo, notasInternas }) {
  const { data } = await api.post(`/admin/citas/${id}/completar`, {
    resumenTrabajo: resumenTrabajo || null,
    notasInternas: notasInternas || null,
  })
  return data
}