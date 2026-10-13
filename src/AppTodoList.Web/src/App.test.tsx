import { afterEach, describe, expect, it, vi } from 'vitest'
import { cleanup, render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import App from './App'

afterEach(() => {
  cleanup()
  vi.unstubAllGlobals()
})

describe('AppTodoList', () => {
  it('muestra la lista vacía y permite crear una tarea persistida', async () => {
    const fetchMock = vi.fn<typeof fetch>()
    const tarea = {
      id: 1,
      title: 'Preparar la demo',
      description: null,
      status: 'Pending',
      priority: 'Medium',
      createdAt: '2026-10-13T11:00:00Z',
      updatedAt: '2026-10-13T11:00:00Z',
      dueDate: null,
    }
    fetchMock
      .mockResolvedValueOnce(respuestaJson({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 }))
      .mockResolvedValueOnce(respuestaJson(tarea, 201))
      .mockResolvedValueOnce(
        respuestaJson({ items: [tarea], page: 1, pageSize: 10, totalCount: 1, totalPages: 1 }),
      )
    vi.stubGlobal('fetch', fetchMock)

    const usuario = userEvent.setup()
    render(<App />)

    expect(await screen.findByRole('heading', { name: 'Tu lista está despejada' })).toBeVisible()
    await usuario.click(screen.getByRole('button', { name: 'Crear primera tarea' }))
    await usuario.type(screen.getByLabelText(/Título/), 'Preparar la demo')
    await usuario.click(screen.getByRole('button', { name: 'Crear tarea' }))

    expect(await screen.findByText('La tarea se ha creado.')).toBeVisible()
    expect(await screen.findByRole('heading', { name: 'Preparar la demo' })).toBeVisible()
    expect(fetchMock).toHaveBeenCalledTimes(3)
    expect(fetchMock.mock.calls[1]?.[1]?.method).toBe('POST')
  })

  it('informa del error de carga y deja reintentar', async () => {
    const fetchMock = vi
      .fn<typeof fetch>()
      .mockResolvedValueOnce(
        respuestaJson(
          { title: 'Servicio no disponible' },
          503,
          'application/problem+json',
        ),
      )
      .mockResolvedValueOnce(
        respuestaJson({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 }),
      )
    vi.stubGlobal('fetch', fetchMock)

    const usuario = userEvent.setup()
    render(<App />)

    expect(await screen.findByRole('alert')).toHaveTextContent('Servicio no disponible')
    await usuario.click(screen.getByRole('button', { name: 'Volver a intentar' }))
    expect(await screen.findByRole('heading', { name: 'Tu lista está despejada' })).toBeVisible()
    expect(fetchMock).toHaveBeenCalledTimes(2)
  })

  it('no elimina sin confirmación y actualiza la lista tras confirmar', async () => {
    const tarea = {
      id: 7,
      title: 'Revisar el informe',
      description: null,
      status: 'Pending',
      priority: 'Medium',
      createdAt: '2026-10-13T11:00:00Z',
      updatedAt: '2026-10-13T11:00:00Z',
      dueDate: null,
    }
    const fetchMock = vi
      .fn<typeof fetch>()
      .mockResolvedValueOnce(
        respuestaJson({ items: [tarea], page: 1, pageSize: 10, totalCount: 1, totalPages: 1 }),
      )
      .mockResolvedValueOnce(new Response(null, { status: 204 }))
      .mockResolvedValueOnce(
        respuestaJson({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 }),
      )
    vi.stubGlobal('fetch', fetchMock)
    vi.spyOn(window, 'confirm').mockReturnValueOnce(false).mockReturnValueOnce(true)

    const usuario = userEvent.setup()
    render(<App />)

    await usuario.click(await screen.findByRole('button', { name: 'Eliminar Revisar el informe' }))
    expect(fetchMock).toHaveBeenCalledTimes(1)

    await usuario.click(screen.getByRole('button', { name: 'Eliminar Revisar el informe' }))
    expect(await screen.findByText('La tarea se ha eliminado.')).toBeVisible()
    expect(await screen.findByRole('heading', { name: 'Tu lista está despejada' })).toBeVisible()
    expect(fetchMock).toHaveBeenCalledTimes(3)
    expect(fetchMock.mock.calls[1]?.[1]?.method).toBe('DELETE')
  })

  it('consulta y muestra los datos completos de una tarea', async () => {
    const tarea = {
      id: 9,
      title: 'Consultar detalles',
      description: 'Descripción de la tarea',
      status: 'InProgress',
      priority: 'High',
      createdAt: '2026-10-13T11:00:00Z',
      updatedAt: '2026-10-13T12:00:00Z',
      dueDate: '2026-10-14T13:30:00Z',
    }
    const fetchMock = vi
      .fn<typeof fetch>()
      .mockResolvedValueOnce(
        respuestaJson({ items: [tarea], page: 1, pageSize: 10, totalCount: 1, totalPages: 1 }),
      )
      .mockResolvedValueOnce(respuestaJson(tarea))
    vi.stubGlobal('fetch', fetchMock)

    const usuario = userEvent.setup()
    render(<App />)

    await usuario.click(
      await screen.findByRole('button', { name: 'Ver detalle de Consultar detalles' }),
    )

    const panelDetalle = await screen.findByRole('region', {
      name: 'Detalle de Consultar detalles',
    })
    expect(within(panelDetalle).getByText('Última actualización')).toBeVisible()
    expect(within(panelDetalle).getByText('En progreso')).toBeVisible()
    expect(fetchMock.mock.calls[1]?.[0]).toBe('/api/tasks/9')
  })
})

function respuestaJson(
  contenido: unknown,
  estado = 200,
  tipo = 'application/json',
): Response {
  return new Response(JSON.stringify(contenido), {
    status: estado,
    headers: { 'Content-Type': tipo },
  })
}
