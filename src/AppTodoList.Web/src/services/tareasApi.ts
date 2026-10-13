import {
  estadosTarea,
  prioridadesTarea,
  type DatosTarea,
  type EstadoTarea,
  type FiltrosTareas,
  type PaginaTareas,
  type PrioridadTarea,
  type Tarea,
} from '../types'

export async function listarTareas(
  filtros: FiltrosTareas,
  signal?: AbortSignal,
): Promise<PaginaTareas> {
  const parametros = new URLSearchParams({
    page: String(filtros.page),
    pageSize: String(filtros.pageSize),
    sortBy: filtros.sortBy,
    sortDirection: filtros.sortDirection,
  })

  if (filtros.search.trim()) {
    parametros.set('search', filtros.search.trim())
  }
  if (filtros.status) {
    parametros.set('status', filtros.status)
  }
  if (filtros.priority) {
    parametros.set('priority', filtros.priority)
  }

  const respuesta = await solicitar(`/api/tasks?${parametros.toString()}`, { signal })
  const contenido: unknown = await respuesta.json()

  if (!esPaginaTareas(contenido)) {
    throw new Error('La API devolvió una lista de tareas con un formato no válido.')
  }

  return contenido
}

export async function crearTarea(datos: DatosTarea): Promise<Tarea> {
  return leerTarea(
    await solicitar('/api/tasks', {
      method: 'POST',
      body: JSON.stringify(datos),
    }),
  )
}

export async function obtenerTarea(id: number): Promise<Tarea> {
  return leerTarea(await solicitar(`/api/tasks/${id}`))
}

export async function actualizarTarea(id: number, datos: DatosTarea): Promise<Tarea> {
  return leerTarea(
    await solicitar(`/api/tasks/${id}`, {
      method: 'PUT',
      body: JSON.stringify(datos),
    }),
  )
}

export async function cambiarEstadoTarea(
  id: number,
  status: EstadoTarea,
): Promise<Tarea> {
  return leerTarea(
    await solicitar(`/api/tasks/${id}/status`, {
      method: 'PATCH',
      body: JSON.stringify({ status }),
    }),
  )
}

export async function eliminarTarea(id: number): Promise<void> {
  await solicitar(`/api/tasks/${id}`, { method: 'DELETE' })
}

async function solicitar(url: string, opciones: RequestInit = {}): Promise<Response> {
  const headers = new Headers(opciones.headers)
  headers.set('Accept', 'application/json')

  if (opciones.body !== undefined) {
    headers.set('Content-Type', 'application/json')
  }

  const respuesta = await fetch(url, { ...opciones, headers })

  if (!respuesta.ok) {
    throw await crearErrorApi(respuesta)
  }

  return respuesta
}

async function crearErrorApi(respuesta: Response): Promise<Error> {
  const mensajeGenerico = `La solicitud no se completó (HTTP ${respuesta.status}).`
  const tipoContenido = respuesta.headers.get('content-type') ?? ''

  if (!tipoContenido.includes('json')) {
    return new Error(mensajeGenerico)
  }

  let contenido: unknown
  try {
    contenido = await respuesta.json()
  } catch (error: unknown) {
    if (error instanceof SyntaxError) {
      return new Error(
        `La API respondió con el código ${respuesta.status} y un error con formato no válido.`,
      )
    }

    throw error
  }

  if (!esRegistro(contenido)) {
    return new Error(mensajeGenerico)
  }

  const erroresValidacion = extraerErrorValidacion(contenido.errors)
  if (erroresValidacion) {
    return new Error(erroresValidacion)
  }

  if (typeof contenido.detail === 'string') {
    return new Error(contenido.detail)
  }
  if (typeof contenido.title === 'string') {
    return new Error(contenido.title)
  }

  return new Error(mensajeGenerico)
}

async function leerTarea(respuesta: Response): Promise<Tarea> {
  const contenido: unknown = await respuesta.json()

  if (!esTarea(contenido)) {
    throw new Error('La API devolvió una tarea con un formato no válido.')
  }

  return contenido
}

function extraerErrorValidacion(errores: unknown): string | null {
  if (!esRegistro(errores)) {
    return null
  }

  for (const mensajes of Object.values(errores)) {
    if (!Array.isArray(mensajes)) {
      continue
    }

    const mensaje = mensajes.find((valor): valor is string => typeof valor === 'string')
    if (mensaje) {
      return mensaje
    }
  }

  return null
}

function esPaginaTareas(valor: unknown): valor is PaginaTareas {
  return (
    esRegistro(valor) &&
    Array.isArray(valor.items) &&
    valor.items.every(esTarea) &&
    Number.isInteger(valor.page) &&
    Number(valor.page) >= 1 &&
    Number.isInteger(valor.pageSize) &&
    Number(valor.pageSize) >= 1 &&
    Number.isInteger(valor.totalCount) &&
    Number(valor.totalCount) >= 0 &&
    Number.isInteger(valor.totalPages) &&
    Number(valor.totalPages) >= 0
  )
}

function esTarea(valor: unknown): valor is Tarea {
  return (
    esRegistro(valor) &&
    Number.isInteger(valor.id) &&
    typeof valor.title === 'string' &&
    (typeof valor.description === 'string' || valor.description === null) &&
    esEstadoTarea(valor.status) &&
    esPrioridadTarea(valor.priority) &&
    esFecha(valor.createdAt) &&
    esFecha(valor.updatedAt) &&
    (valor.dueDate === null || esFecha(valor.dueDate))
  )
}

function esRegistro(valor: unknown): valor is Record<string, unknown> {
  return typeof valor === 'object' && valor !== null
}

function esFecha(valor: unknown): valor is string {
  return typeof valor === 'string' && Number.isFinite(Date.parse(valor))
}

function esEstadoTarea(valor: unknown): valor is EstadoTarea {
  return estadosTarea.some((estado) => estado.valor === valor)
}

function esPrioridadTarea(valor: unknown): valor is PrioridadTarea {
  return prioridadesTarea.some((prioridad) => prioridad.valor === valor)
}
