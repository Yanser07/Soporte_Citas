export function mensajeDeError(err) {
  const data = err?.response?.data

  if (data?.mensaje) return data.mensaje
  if (data?.errors) return Object.values(data.errors).flat().join(' ')
  if (!err?.response) return 'No se pudo conectar con el servidor.'

  return 'Ocurrió un error inesperado.'
}