import { useEffect, useState, type FormEvent } from 'react'
import {
  actualizarTarea,
  cambiarEstadoTarea,
  crearTarea,
  eliminarTarea,
  listarTareas,
  obtenerTarea,
} from './services/tareasApi'
import {
  camposOrdenTareas,
  estadosTarea,
  prioridadesTarea,
  type DatosTarea,
  type DireccionOrden,
  type EstadoTarea,
  type FiltrosTareas,
  type PaginaTareas,
  type PrioridadTarea,
  type Tarea,
} from './types'
import './App.css'

interface CamposFormulario {
  title: string
  description: string
  priority: PrioridadTarea
  dueDate: string
}

const filtrosIniciales: FiltrosTareas = {
  page: 1,
  pageSize: 10,
  search: '',
  status: '',
  priority: '',
  sortBy: 'createdAt',
  sortDirection: 'desc',
}

const formularioInicial: CamposFormulario = {
  title: '',
  description: '',
  priority: 'Medium',
  dueDate: '',
}

function App() {
  const [filtros, setFiltros] = useState<FiltrosTareas>(filtrosIniciales)
  const [pagina, setPagina] = useState<PaginaTareas | null>(null)
  const [cargando, setCargando] = useState(true)
  const [errorCarga, setErrorCarga] = useState<string | null>(null)
  const [errorOperacion, setErrorOperacion] = useState<string | null>(null)
  const [mensajeExito, setMensajeExito] = useState<string | null>(null)
  const [intento, setIntento] = useState(0)
  const [formularioAbierto, setFormularioAbierto] = useState(false)
  const [tareaEditada, setTareaEditada] = useState<Tarea | null>(null)
  const [campos, setCampos] = useState<CamposFormulario>(formularioInicial)
  const [errorFormulario, setErrorFormulario] = useState<string | null>(null)
  const [guardando, setGuardando] = useState(false)
  const [tareaEliminando, setTareaEliminando] = useState<number | null>(null)
  const [tareaActualizandoEstado, setTareaActualizandoEstado] = useState<number | null>(
    null,
  )
  const [detalleId, setDetalleId] = useState<number | null>(null)
  const [tareaDetalle, setTareaDetalle] = useState<Tarea | null>(null)
  const [detalleCargando, setDetalleCargando] = useState(false)
  const [errorDetalle, setErrorDetalle] = useState<string | null>(null)

  useEffect(() => {
    const controlador = new AbortController()

    void listarTareas(filtros, controlador.signal)
      .then(setPagina)
      .catch((error: unknown) => {
        if (!esErrorCancelacion(error)) {
          setErrorCarga(mensajeDeError(error))
        }
      })
      .finally(() => {
        if (!controlador.signal.aborted) {
          setCargando(false)
        }
      })

    return () => controlador.abort()
  }, [filtros, intento])

  const tareas = pagina?.items ?? []
  const paginasTotales = Math.max(pagina?.totalPages ?? 0, 1)
  const hayFiltrosActivos = Boolean(filtros.search || filtros.status || filtros.priority)

  function actualizarFiltros(cambios: Partial<FiltrosTareas>) {
    setCargando(true)
    setErrorCarga(null)
    setDetalleId(null)
    setTareaDetalle(null)
    setErrorDetalle(null)
    setFiltros((actuales) => ({
      ...actuales,
      ...cambios,
      page: cambios.page ?? 1,
    }))
    setMensajeExito(null)
  }

  function recargarLista() {
    setCargando(true)
    setErrorCarga(null)
    setIntento((actual) => actual + 1)
  }

  async function cargarDetalle(id: number) {
    setDetalleId(id)
    setTareaDetalle(null)
    setErrorDetalle(null)
    setDetalleCargando(true)

    try {
      setTareaDetalle(await obtenerTarea(id))
    } catch (error: unknown) {
      setErrorDetalle(mensajeDeError(error))
    } finally {
      setDetalleCargando(false)
    }
  }

  function alternarDetalle(id: number) {
    if (detalleId === id) {
      setDetalleId(null)
      setTareaDetalle(null)
      setErrorDetalle(null)
      return
    }

    void cargarDetalle(id)
  }

  function abrirFormularioNuevo() {
    setTareaEditada(null)
    setCampos(formularioInicial)
    setErrorFormulario(null)
    setFormularioAbierto(true)
    setErrorOperacion(null)
    setMensajeExito(null)
  }

  function abrirFormularioEdicion(tarea: Tarea) {
    setTareaEditada(tarea)
    setCampos({
      title: tarea.title,
      description: tarea.description ?? '',
      priority: tarea.priority,
      dueDate: fechaParaFormulario(tarea.dueDate),
    })
    setErrorFormulario(null)
    setFormularioAbierto(true)
    setErrorOperacion(null)
    setMensajeExito(null)
  }

  function cerrarFormulario() {
    setFormularioAbierto(false)
    setTareaEditada(null)
    setErrorFormulario(null)
  }

  async function guardarTarea(evento: FormEvent<HTMLFormElement>) {
    evento.preventDefault()
    const titulo = campos.title.trim()

    if (!titulo) {
      setErrorFormulario('El título es obligatorio.')
      return
    }

    const vencimiento = campos.dueDate ? new Date(campos.dueDate) : null
    if (vencimiento && !Number.isFinite(vencimiento.getTime())) {
      setErrorFormulario('Introduce una fecha de vencimiento válida.')
      return
    }

    const datos: DatosTarea = {
      title: titulo,
      description: campos.description.trim(),
      priority: campos.priority,
      dueDate: vencimiento?.toISOString() ?? null,
    }

    setGuardando(true)
    setErrorFormulario(null)
    setErrorOperacion(null)
    setMensajeExito(null)

    try {
      if (tareaEditada) {
        await actualizarTarea(tareaEditada.id, datos)
        setMensajeExito('La tarea se ha actualizado.')
      } else {
        await crearTarea(datos)
        setMensajeExito('La tarea se ha creado.')
      }

      cerrarFormulario()
      setCampos(formularioInicial)
      recargarLista()
    } catch (error: unknown) {
      setErrorOperacion(mensajeDeError(error))
    } finally {
      setGuardando(false)
    }
  }

  async function actualizarEstado(tarea: Tarea, estado: EstadoTarea) {
    setTareaActualizandoEstado(tarea.id)
    setErrorOperacion(null)
    setMensajeExito(null)

    try {
      await cambiarEstadoTarea(tarea.id, estado)
      setMensajeExito('El estado de la tarea se ha actualizado.')
      recargarLista()
    } catch (error: unknown) {
      setErrorOperacion(mensajeDeError(error))
    } finally {
      setTareaActualizandoEstado(null)
    }
  }

  async function eliminar(tarea: Tarea) {
    const confirmado = window.confirm(`¿Quieres eliminar «${tarea.title}»?`)
    if (!confirmado) {
      return
    }

    setTareaEliminando(tarea.id)
    setErrorOperacion(null)
    setMensajeExito(null)

    try {
      await eliminarTarea(tarea.id)
      setMensajeExito('La tarea se ha eliminado.')

      if (tareas.length === 1 && filtros.page > 1) {
        setCargando(true)
        setErrorCarga(null)
        setFiltros((actuales) => ({ ...actuales, page: actuales.page - 1 }))
      } else {
        recargarLista()
      }
    } catch (error: unknown) {
      setErrorOperacion(mensajeDeError(error))
    } finally {
      setTareaEliminando(null)
    }
  }

  function limpiarFiltros() {
    setCargando(true)
    setErrorCarga(null)
    setFiltros({ ...filtrosIniciales, pageSize: filtros.pageSize })
    setMensajeExito(null)
  }

  return (
    <div className="app-shell">
      <header className="topbar">
        <a className="brand" href="/" aria-label="AppTodoList, inicio">
          <span className="brand-mark" aria-hidden="true">
            ✓
          </span>
          <span>AppTodoList</span>
        </a>
        <span className="shared-list-label">
          <span className="online-dot" aria-hidden="true" />
          Lista compartida
        </span>
      </header>

      <main className="workspace">
        <section className="intro">
          <p className="eyebrow">ORGANIZA · PRIORIZA · AVANZA</p>
          <h1>Tus tareas, bajo control.</h1>
          <p className="intro-copy">
            Un espacio sencillo para tener claro qué toca hacer y dar el siguiente paso.
          </p>
        </section>

        {errorOperacion && (
          <div className="notice notice-error" role="alert">
            <span>{errorOperacion}</span>
            <button
              className="notice-dismiss"
              type="button"
              aria-label="Cerrar aviso de error"
              onClick={() => setErrorOperacion(null)}
            >
              ×
            </button>
          </div>
        )}
        {mensajeExito && (
          <p className="notice notice-success" role="status" aria-live="polite">
            <span aria-hidden="true">✓</span>
            {mensajeExito}
          </p>
        )}

        {formularioAbierto && (
          <section className="panel form-panel" aria-labelledby="formulario-titulo">
            <div className="panel-heading">
              <div>
                <p className="eyebrow">DETALLES DE LA TAREA</p>
                <h2 id="formulario-titulo">
                  {tareaEditada ? 'Editar tarea' : 'Nueva tarea'}
                </h2>
              </div>
              <button
                className="icon-button"
                type="button"
                aria-label="Cerrar formulario"
                onClick={cerrarFormulario}
                disabled={guardando}
              >
                ×
              </button>
            </div>

            <form className="task-form" onSubmit={guardarTarea}>
              <label className="field field-full">
                <span>Título <span className="required-mark">(obligatorio)</span></span>
                <input
                  autoFocus
                  required
                  maxLength={200}
                  value={campos.title}
                  onChange={(evento) => {
                    const title = evento.currentTarget.value
                    setCampos((actuales) => ({
                      ...actuales,
                      title,
                    }))
                  }}
                  placeholder="Por ejemplo, preparar la presentación"
                />
              </label>

              <label className="field field-full">
                <span>Descripción <span className="optional-mark">(opcional)</span></span>
                <textarea
                  rows={3}
                  maxLength={2000}
                  value={campos.description}
                  onChange={(evento) => {
                    const description = evento.currentTarget.value
                    setCampos((actuales) => ({
                      ...actuales,
                      description,
                    }))
                  }}
                  placeholder="Añade contexto o los siguientes pasos"
                />
              </label>

              <label className="field">
                <span>Prioridad</span>
                <select
                  value={campos.priority}
                  onChange={(evento) => {
                    const prioridad = prioridadesTarea.find(
                      (opcion) => opcion.valor === evento.currentTarget.value,
                    )
                    if (prioridad) {
                      setCampos((actuales) => ({
                        ...actuales,
                        priority: prioridad.valor,
                      }))
                    }
                  }}
                >
                  {prioridadesTarea.map((prioridad) => (
                    <option key={prioridad.valor} value={prioridad.valor}>
                      {prioridad.etiqueta}
                    </option>
                  ))}
                </select>
              </label>

              <label className="field">
                <span>Fecha de vencimiento <span className="optional-mark">(opcional)</span></span>
                <input
                  type="datetime-local"
                  value={campos.dueDate}
                  onChange={(evento) => {
                    const dueDate = evento.currentTarget.value
                    setCampos((actuales) => ({
                      ...actuales,
                      dueDate,
                    }))
                  }}
                />
              </label>

              {errorFormulario && (
                <p className="form-error field-full" role="alert">
                  {errorFormulario}
                </p>
              )}

              <div className="form-actions field-full">
                <button
                  className="button button-secondary"
                  type="button"
                  onClick={cerrarFormulario}
                  disabled={guardando}
                >
                  Cancelar
                </button>
                <button className="button button-primary" type="submit" disabled={guardando}>
                  {guardando ? 'Guardando…' : tareaEditada ? 'Guardar cambios' : 'Crear tarea'}
                </button>
              </div>
            </form>
          </section>
        )}

        <section className="panel list-panel" aria-labelledby="lista-titulo">
          <div className="panel-heading list-heading">
            <div>
              <p className="eyebrow">TU ESPACIO DE TRABAJO</p>
              <h2 id="lista-titulo">Lista de tareas</h2>
              <p className="list-count">
                {pagina
                  ? `${pagina.totalCount} ${
                      pagina.totalCount === 1 ? 'tarea en total' : 'tareas en total'
                    }`
                  : 'Cargando tu lista'}
              </p>
            </div>
            <button
              className="button button-primary"
              type="button"
              onClick={abrirFormularioNuevo}
            >
              <span aria-hidden="true">+</span>
              Nueva tarea
            </button>
          </div>

          <div className="filters" aria-label="Buscar, filtrar y ordenar tareas">
            <label className="field search-field">
              <span>Buscar</span>
              <span className="search-input-wrap">
                <span className="search-icon" aria-hidden="true">⌕</span>
                <input
                  type="search"
                  maxLength={200}
                  value={filtros.search}
                  onChange={(evento) => actualizarFiltros({ search: evento.currentTarget.value })}
                  placeholder="Título o descripción"
                />
              </span>
            </label>

            <label className="field">
              <span>Estado</span>
              <select
                aria-label="Filtrar por estado"
                value={filtros.status}
                onChange={(evento) => {
                  const estado = estadosTarea.find(
                    (opcion) => opcion.valor === evento.currentTarget.value,
                  )
                  actualizarFiltros({ status: estado?.valor ?? '' })
                }}
              >
                <option value="">Todos</option>
                {estadosTarea.map((estado) => (
                  <option key={estado.valor} value={estado.valor}>
                    {estado.etiqueta}
                  </option>
                ))}
              </select>
            </label>

            <label className="field">
              <span>Prioridad</span>
              <select
                aria-label="Filtrar por prioridad"
                value={filtros.priority}
                onChange={(evento) => {
                  const prioridad = prioridadesTarea.find(
                    (opcion) => opcion.valor === evento.currentTarget.value,
                  )
                  actualizarFiltros({ priority: prioridad?.valor ?? '' })
                }}
              >
                <option value="">Todas</option>
                {prioridadesTarea.map((prioridad) => (
                  <option key={prioridad.valor} value={prioridad.valor}>
                    {prioridad.etiqueta}
                  </option>
                ))}
              </select>
            </label>

            <label className="field">
              <span>Ordenar por</span>
              <select
                value={filtros.sortBy}
                onChange={(evento) => {
                  const campo = camposOrdenTareas.find(
                    (opcion) => opcion.valor === evento.currentTarget.value,
                  )
                  if (campo) {
                    actualizarFiltros({ sortBy: campo.valor })
                  }
                }}
              >
                {camposOrdenTareas.map((campo) => (
                  <option key={campo.valor} value={campo.valor}>
                    {campo.etiqueta}
                  </option>
                ))}
              </select>
            </label>

            <label className="field">
              <span>Dirección</span>
              <select
                value={filtros.sortDirection}
                onChange={(evento) => {
                  const direccion: DireccionOrden =
                    evento.currentTarget.value === 'asc' ? 'asc' : 'desc'
                  actualizarFiltros({ sortDirection: direccion })
                }}
              >
                <option value="desc">Descendente</option>
                <option value="asc">Ascendente</option>
              </select>
            </label>

            <label className="field page-size-field">
              <span>Por página</span>
              <select
                value={filtros.pageSize}
                onChange={(evento) =>
                  actualizarFiltros({ pageSize: Number(evento.currentTarget.value) })
                }
              >
                <option value={10}>10</option>
                <option value={20}>20</option>
                <option value={50}>50</option>
              </select>
            </label>
          </div>

          <div className="task-list-region">
            {errorCarga ? (
              <div className="load-state error-state" role="alert">
                <span className="state-icon" aria-hidden="true">!</span>
                <h3>No se ha podido cargar la lista</h3>
                <p>{errorCarga}</p>
                <button
                  className="button button-secondary"
                  type="button"
                  onClick={recargarLista}
                >
                  Volver a intentar
                </button>
              </div>
            ) : cargando ? (
              <div className="load-state" role="status" aria-live="polite">
                <span className="spinner" aria-hidden="true" />
                <p>Cargando tareas…</p>
              </div>
            ) : tareas.length === 0 ? (
              <div className="load-state empty-state">
                <span className="state-icon" aria-hidden="true">
                  {hayFiltrosActivos ? '⌕' : '✓'}
                </span>
                <h3>{hayFiltrosActivos ? 'No hay resultados' : 'Tu lista está despejada'}</h3>
                <p>
                  {hayFiltrosActivos
                    ? 'Prueba a cambiar o limpiar los filtros de búsqueda.'
                    : 'Crea una tarea y aparecerá aquí con su estado y prioridad.'}
                </p>
                {hayFiltrosActivos ? (
                  <button
                    className="button button-secondary"
                    type="button"
                    onClick={limpiarFiltros}
                  >
                    Limpiar filtros
                  </button>
                ) : (
                  <button
                    className="button button-primary"
                    type="button"
                    onClick={abrirFormularioNuevo}
                  >
                    Crear primera tarea
                  </button>
                )}
              </div>
            ) : (
              <ul className="task-list">
                {tareas.map((tarea) => (
                  <li key={tarea.id}>
                    <article className="task-card">
                      <div className="task-content">
                        <div className="task-title-row">
                          <h3>{tarea.title}</h3>
                          <span className={`priority-badge priority-${tarea.priority.toLowerCase()}`}>
                            {etiquetaPrioridad(tarea.priority)}
                          </span>
                        </div>
                        {tarea.description && <p className="task-description">{tarea.description}</p>}
                        <div className="task-meta">
                          <span>
                            Vence:{' '}
                            {tarea.dueDate ? (
                              <time dateTime={tarea.dueDate}>
                                {formatearFecha(tarea.dueDate)}
                              </time>
                            ) : (
                              'Sin fecha'
                            )}
                          </span>
                        </div>
                      </div>

                      <div className="task-actions">
                        <label className="sr-only" htmlFor={`estado-${tarea.id}`}>
                          Estado de {tarea.title}
                        </label>
                        <select
                          id={`estado-${tarea.id}`}
                          className={`status-select status-${tarea.status.toLowerCase()}`}
                          value={tarea.status}
                          disabled={
                            tareaActualizandoEstado === tarea.id ||
                            tareaEliminando === tarea.id
                          }
                          onChange={(evento) => {
                            const estado = estadosTarea.find(
                              (opcion) => opcion.valor === evento.currentTarget.value,
                            )
                            if (estado) {
                              void actualizarEstado(tarea, estado.valor)
                            }
                          }}
                        >
                          {estadosTarea.map((estado) => (
                            <option key={estado.valor} value={estado.valor}>
                              {estado.etiqueta}
                            </option>
                          ))}
                        </select>
                        <button
                          className="text-button"
                          type="button"
                          aria-label={`Ver detalle de ${tarea.title}`}
                          aria-expanded={detalleId === tarea.id}
                          aria-controls={
                            detalleId === tarea.id ? `detalle-${tarea.id}` : undefined
                          }
                          disabled={detalleCargando}
                          onClick={() => alternarDetalle(tarea.id)}
                        >
                          {detalleId === tarea.id ? 'Ocultar detalle' : 'Ver detalle'}
                        </button>
                        <button
                          className="text-button"
                          type="button"
                          aria-label={`Editar ${tarea.title}`}
                          disabled={tareaEliminando === tarea.id}
                          onClick={() => abrirFormularioEdicion(tarea)}
                        >
                          Editar
                        </button>
                        <button
                          className="text-button text-button-danger"
                          type="button"
                          aria-label={`Eliminar ${tarea.title}`}
                          disabled={
                            tareaEliminando === tarea.id ||
                            tareaActualizandoEstado === tarea.id
                          }
                          onClick={() => void eliminar(tarea)}
                        >
                          {tareaEliminando === tarea.id ? 'Eliminando…' : 'Eliminar'}
                        </button>
                      </div>
                    </article>
                    {detalleId === tarea.id && (
                      <section
                        className="details-panel"
                        id={`detalle-${tarea.id}`}
                        aria-label={`Detalle de ${tarea.title}`}
                      >
                        {detalleCargando ? (
                          <p role="status">Cargando detalle…</p>
                        ) : errorDetalle ? (
                          <div className="details-error" role="alert">
                            <span>{errorDetalle}</span>
                            <button
                              className="text-button"
                              type="button"
                              onClick={() => void cargarDetalle(tarea.id)}
                            >
                              Volver a intentar
                            </button>
                          </div>
                        ) : tareaDetalle ? (
                          <dl className="details-grid">
                            <div>
                              <dt>Estado</dt>
                              <dd>{etiquetaEstado(tareaDetalle.status)}</dd>
                            </div>
                            <div>
                              <dt>Prioridad</dt>
                              <dd>{etiquetaPrioridad(tareaDetalle.priority)}</dd>
                            </div>
                            <div>
                              <dt>Creada</dt>
                              <dd>
                                <time dateTime={tareaDetalle.createdAt}>
                                  {formatearFecha(tareaDetalle.createdAt)}
                                </time>
                              </dd>
                            </div>
                            <div>
                              <dt>Última actualización</dt>
                              <dd>
                                <time dateTime={tareaDetalle.updatedAt}>
                                  {formatearFecha(tareaDetalle.updatedAt)}
                                </time>
                              </dd>
                            </div>
                            <div>
                              <dt>Vencimiento</dt>
                              <dd>
                                {tareaDetalle.dueDate
                                  ? formatearFecha(tareaDetalle.dueDate)
                                  : 'Sin fecha'}
                              </dd>
                            </div>
                          </dl>
                        ) : null}
                      </section>
                    )}
                  </li>
                ))}
              </ul>
            )}
          </div>

          <footer className="pagination">
            <span className="pagination-count" aria-live="polite">
              {pagina
                ? `Mostrando ${tareas.length} de ${pagina.totalCount} ${
                    pagina.totalCount === 1 ? 'tarea' : 'tareas'
                  }`
                : ''}
            </span>
            <div className="pagination-controls">
              <button
                className="button button-page"
                type="button"
                aria-label="Página anterior"
                disabled={cargando || filtros.page <= 1 || paginasTotales <= 1}
                onClick={() => actualizarFiltros({ page: filtros.page - 1 })}
              >
                Anterior
              </button>
              <span aria-current="page">
                Página {filtros.page} de {paginasTotales}
              </span>
              <button
                className="button button-page"
                type="button"
                aria-label="Página siguiente"
                disabled={cargando || filtros.page >= paginasTotales}
                onClick={() => actualizarFiltros({ page: filtros.page + 1 })}
              >
                Siguiente
              </button>
            </div>
          </footer>
        </section>

        <p className="privacy-note">
          Esta es una lista compartida: cualquier persona con acceso a la aplicación puede
          modificar sus tareas.
        </p>
      </main>
    </div>
  )
}

function etiquetaPrioridad(prioridad: PrioridadTarea): string {
  return prioridadesTarea.find((opcion) => opcion.valor === prioridad)?.etiqueta ?? prioridad
}

function etiquetaEstado(estado: EstadoTarea): string {
  return estadosTarea.find((opcion) => opcion.valor === estado)?.etiqueta ?? estado
}

function fechaParaFormulario(fecha: string | null): string {
  if (!fecha) {
    return ''
  }

  const instante = new Date(fecha)
  if (!Number.isFinite(instante.getTime())) {
    return ''
  }

  const fechaLocal = new Date(instante.getTime() - instante.getTimezoneOffset() * 60_000)
  return fechaLocal.toISOString().slice(0, 16)
}

function formatearFecha(fecha: string): string {
  const instante = new Date(fecha)
  if (!Number.isFinite(instante.getTime())) {
    return 'Fecha no válida'
  }

  return new Intl.DateTimeFormat('es-ES', {
    dateStyle: 'medium',
    timeStyle: 'short',
  }).format(instante)
}

function esErrorCancelacion(error: unknown): boolean {
  return error instanceof Error && error.name === 'AbortError'
}

function mensajeDeError(error: unknown): string {
  return error instanceof Error
    ? error.message
    : 'Ha ocurrido un error inesperado. Inténtalo de nuevo.'
}

export default App
