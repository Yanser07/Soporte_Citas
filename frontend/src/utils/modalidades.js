import { MapPin, Monitor, Store } from 'lucide-vue-next'

export const modalidades = [
  { valor: 'Remoto', etiqueta: 'Remoto', descripcion: 'Por videollamada o acceso remoto', icono: Monitor },
  { valor: 'Local', etiqueta: 'En el local', descripcion: 'Llevas el equipo a nuestro local', icono: Store },
  { valor: 'Domicilio', etiqueta: 'A domicilio', descripcion: 'Vamos a tu casa u oficina', icono: MapPin },
]

export function infoModalidad(valor) {
  return modalidades.find((m) => m.valor === valor) ?? modalidades[1]
}