const formatoFechaHora = new Intl.DateTimeFormat('es-DO', {
  dateStyle: 'full',
  timeStyle: 'short',
})

const formatoHora = new Intl.DateTimeFormat('es-DO', {
  hour: 'numeric',
  minute: '2-digit',
})

export function formatearFechaHora(valor) {
  return formatoFechaHora.format(new Date(valor))
}

export function formatearHora(valor) {
  return formatoHora.format(new Date(valor))
}

export function partesFecha(valor) {
  const fecha = new Date(valor)
  return {
    dia: fecha.getDate(),
    mes: fecha.toLocaleDateString('es-DO', { month: 'short' }).replace('.', ''),
    diaSemana: fecha.toLocaleDateString('es-DO', { weekday: 'long' }),
    hora: formatoHora.format(fecha),
  }
}

// Fecha de hoy en formato yyyy-MM-dd (hora local), para el atributo min del calendario
export function hoyLocal() {
  const d = new Date()
  d.setMinutes(d.getMinutes() - d.getTimezoneOffset())
  return d.toISOString().slice(0, 10)
}

const formatoFecha = new Intl.DateTimeFormat('es-DO', { dateStyle: 'long' })

export function formatearFecha(valor) {
  return formatoFecha.format(new Date(valor))
}

export function etiquetaHora(hora) {
  return `${String(hora).padStart(2, '0')}:00`
}
