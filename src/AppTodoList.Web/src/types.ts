export type EstadoTarea = 'Pending' | 'InProgress' | 'Completed'
export type PrioridadTarea = 'Low' | 'Medium' | 'High'
export type CampoOrdenTareas = 'createdAt' | 'dueDate' | 'priority' | 'status'
export type DireccionOrden = 'asc' | 'desc'

export interface Tarea {
  id: number
  title: string
  description: string | null
  status: EstadoTarea
  priority: PrioridadTarea
  createdAt: string
  updatedAt: string
  dueDate: string | null
}

export interface PaginaTareas {
  items: Tarea[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface FiltrosTareas {
  page: number
  pageSize: number
  search: string
  status: EstadoTarea | ''
  priority: PrioridadTarea | ''
  sortBy: CampoOrdenTareas
  sortDirection: DireccionOrden
}

export interface DatosTarea {
  title: string
  description: string
  priority: PrioridadTarea
  dueDate: string | null
}

export const estadosTarea: ReadonlyArray<{
  valor: EstadoTarea
  etiqueta: string
}> = [
  { valor: 'Pending', etiqueta: 'Pendiente' },
  { valor: 'InProgress', etiqueta: 'En progreso' },
  { valor: 'Completed', etiqueta: 'Completada' },
]

export const prioridadesTarea: ReadonlyArray<{
  valor: PrioridadTarea
  etiqueta: string
}> = [
  { valor: 'Low', etiqueta: 'Baja' },
  { valor: 'Medium', etiqueta: 'Media' },
  { valor: 'High', etiqueta: 'Alta' },
]

export const camposOrdenTareas: ReadonlyArray<{
  valor: CampoOrdenTareas
  etiqueta: string
}> = [
  { valor: 'createdAt', etiqueta: 'Fecha de creación' },
  { valor: 'dueDate', etiqueta: 'Vencimiento' },
  { valor: 'priority', etiqueta: 'Prioridad' },
  { valor: 'status', etiqueta: 'Estado' },
]
